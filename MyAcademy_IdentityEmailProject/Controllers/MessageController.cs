using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAcademy_IdentityEmailProject.Context;
using MyAcademy_IdentityEmailProject.Entities;
using MyAcademy_IdentityEmailProject.Services;
using System.Net;
using System.Text.RegularExpressions;


namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        public MessageController(EmailContext context, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }
        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var values = await _context.Messages
                .Where(x => x.ReceiverEmail == user.Email && !x.IsDraft && !x.IsTrash)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            ViewBag.FavoriteMessageIds = await _context.MessageFavorites
                .Where(x => x.UserId == user.Id)
                .Select(x => x.MessageId)
                .ToListAsync();

            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var message = await _context.Messages
    .Include(x => x.Category)
    .FirstOrDefaultAsync(x =>
        x.MessageId == id &&
        !x.IsTrash &&
        (x.SenderEmail == user.Email ||
         (x.ReceiverEmail == user.Email && !x.IsDraft)));
            if (message == null)
                return NotFound();

            if (!message.IsRead && message.ReceiverEmail == user.Email)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.IsFavorite = await _context.MessageFavorites
                .AnyAsync(x => x.MessageId == id && x.UserId == user.Id);

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(Message message)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            message.SenderEmail = user.Email!;
            message.ReceiverEmail ??= "";
            message.Subject ??= "";
            message.MessageDetail ??= "";
            message.SendDate = DateTime.Now;
            message.IsDraft = true;
            message.IsRead = false;
            message.IsFavorite = false;
            message.IsTrash = false;

            if (!await _context.Categories.AnyAsync(x => x.CategoryId == message.CategoryId))
            {
                ModelState.Clear();
                ModelState.AddModelError("", "Taslak için geçerli bir kategori seçiniz.");
                ViewBag.Categories = new SelectList(
                    _context.Categories.ToList(),
                    "CategoryId",
                    "CategoryName",
                    message.CategoryId);
                return View("CreateMessage", message);
            }

            message.MessageId = 0;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Taslak başarıyla kaydedildi.";
            return RedirectToAction(nameof(Drafts));

        }

        public IActionResult CreateMessage()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMessage(Message message, string? submitAction)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (submitAction == "draft")
            {
                return await SaveDraft(message);
            }

            message.SenderEmail = user.Email;
            message.SendDate = DateTime.Now;
            message.IsRead = false;
            message.IsFavorite = false;
            message.IsDraft = false;
            message.IsTrash = false;

            ModelState.Remove("SenderEmail");
            ModelState.Remove("Category");

            if (string.IsNullOrWhiteSpace(message.ReceiverEmail) ||
                !Regex.IsMatch(message.ReceiverEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("", "Geçerli bir alıcı e-posta adresi giriniz.");
            }

            if (string.IsNullOrWhiteSpace(message.Subject))
                ModelState.AddModelError("", "Konu alanı boş bırakılamaz.");

            if (string.IsNullOrWhiteSpace(message.MessageDetail))
                ModelState.AddModelError("", "Mesaj içeriği boş bırakılamaz.");

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.CategoryId == message.CategoryId);

            if (!categoryExists)
                ModelState.AddModelError("", "Geçerli bir kategori seçiniz.");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _context.Categories.ToList(),
                    "CategoryId",
                    "CategoryName",
                    message.CategoryId);

                return View(message);
            }

            var receiver = await _userManager.FindByEmailAsync(message.ReceiverEmail);

            try
            {
                if (receiver != null)
                {
                    _context.Messages.Add(message);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var plainText = WebUtility.HtmlDecode(
                        Regex.Replace(message.MessageDetail, "<.*?>", " "));

                    await _emailService.SendAsync(
                        message.ReceiverEmail,
                        message.Subject,
                        plainText);

                    _context.Messages.Add(message);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Mesaj başarıyla gönderildi.";
                return RedirectToAction(nameof(SendBox));
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "Mesaj gönderilemedi. Lütfen tekrar deneyiniz.");

                ViewBag.Categories = new SelectList(
                    _context.Categories.ToList(),
                    "CategoryId",
                    "CategoryName",
                    message.CategoryId);

                return View(message);
            }
        }


        [Authorize]
        public async Task<IActionResult> SendBox()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var values = _context.Messages
                .Where(x => x.SenderEmail == user.Email && !x.IsDraft && !x.IsTrash)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> Drafts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var drafts = await _context.Messages.Where(x => x.SenderEmail == user.Email && x.IsDraft && !x.IsTrash).OrderByDescending(x => x.SendDate).ToListAsync();
            return View(drafts);
        }

        [HttpGet]
        public async Task<IActionResult> EditDraft(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var draft = await _context.Messages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MessageId == id && x.SenderEmail == user.Email && x.IsDraft && !x.IsTrash);
            if (draft == null)
                return NotFound();

            SetEditDraftCategories(draft.CategoryId);
            return View(draft);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDraft(Message message, string? submitAction)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var draft = await _context.Messages
                .FirstOrDefaultAsync(x => x.MessageId == message.MessageId && x.SenderEmail == user.Email && x.IsDraft && !x.IsTrash);
            if (draft == null)
                return NotFound();

            message.ReceiverEmail = message.ReceiverEmail?.Trim() ?? "";
            message.Subject = message.Subject?.Trim() ?? "";
            message.MessageDetail ??= "";

            ModelState.Clear();

            var categoryExists = await _context.Categories.AnyAsync(x => x.CategoryId == message.CategoryId);
            if (submitAction != "draft" && submitAction != "send")
                return BadRequest();
            var isDraft = submitAction == "draft";

            if (!categoryExists)
                ModelState.AddModelError(nameof(Message.CategoryId),
                    isDraft ? "Taslak için geçerli bir kategori seçiniz." : "Geçerli bir kategori seçiniz.");

            if (!isDraft)
            {
                if (string.IsNullOrWhiteSpace(message.ReceiverEmail) ||
                    !Regex.IsMatch(message.ReceiverEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    ModelState.AddModelError(nameof(Message.ReceiverEmail), "Geçerli bir alıcı e-posta adresi giriniz.");

                if (string.IsNullOrWhiteSpace(message.Subject))
                    ModelState.AddModelError(nameof(Message.Subject), "Konu alanı boş bırakılamaz.");

                var plainCheck = WebUtility.HtmlDecode(Regex.Replace(message.MessageDetail, "<.*?>", " "));
                if (string.IsNullOrWhiteSpace(plainCheck))
                    ModelState.AddModelError(nameof(Message.MessageDetail), "Mesaj içeriği boş bırakılamaz.");
            }

            if (!ModelState.IsValid)
            {
                SetEditDraftCategories(message.CategoryId);
                return View(message);
            }

            draft.ReceiverEmail = message.ReceiverEmail;
            draft.Subject = message.Subject;
            draft.MessageDetail = message.MessageDetail;
            draft.CategoryId = message.CategoryId;
            draft.SendDate = DateTime.Now;

            if (isDraft)
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Taslak başarıyla güncellendi.";
                return RedirectToAction(nameof(Drafts));
            }
            try
            {
                var receiver = await _userManager.FindByEmailAsync(draft.ReceiverEmail);
                if (receiver == null)
                {
                    var plainText = WebUtility.HtmlDecode(Regex.Replace(draft.MessageDetail, "<.*?>", " "));
                    await _emailService.SendAsync(draft.ReceiverEmail, draft.Subject, plainText);
                }

                draft.IsDraft = false;
                draft.IsRead = false;
                draft.IsFavorite = false;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mesaj başarıyla gönderildi.";
                return RedirectToAction(nameof(SendBox));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Mesaj gönderilemedi. Lütfen tekrar deneyiniz.");
                SetEditDraftCategories(message.CategoryId);
                return View(message);
            }
        }

        private void SetEditDraftCategories(int selectedCategoryId)
        {
            ViewBag.Categories = new SelectList(
                _context.Categories.ToList(),
                "CategoryId",
                "CategoryName",
                selectedCategoryId);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();
            var message = await _context.Messages.FirstOrDefaultAsync(x => x.MessageId == id &&   !x.IsDraft &&!x.IsTrash &&(x.SenderEmail == user.Email || x.ReceiverEmail == user.Email));
            if (message == null)
                return NotFound();
            var favorite = await _context.MessageFavorites.FirstOrDefaultAsync(x => x.MessageId == id && x.UserId == user.Id);
            if (favorite == null)
            {
                _context.MessageFavorites.Add(new MessageFavorite
                {
                    MessageId = id,
                    UserId = user.Id
                });
            }
            else
            {
                _context.MessageFavorites.Remove(favorite);
            }

            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { isFavorite = favorite == null });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Inbox));
        }

        [HttpGet]
        public async Task<IActionResult> StarredMessages()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var values = await _context.MessageFavorites.Where(x => x.UserId == user.Id &&!x.Message.IsDraft &&!x.Message.IsTrash && (x.Message.SenderEmail == user.Email || x.Message.ReceiverEmail == user.Email)).OrderByDescending(x => x.Message.SendDate).Select(x => x.Message).ToListAsync();
            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> Trash()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var messages = await _context.Messages
                .Where(x => x.IsTrash && !x.IsDraft &&
                            (x.SenderEmail == user.Email || x.ReceiverEmail == user.Email))
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToTrash(int id, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (id <= 0) return BadRequest();

            var message = await _context.Messages.FirstOrDefaultAsync(x =>
                x.MessageId == id &&
                !x.IsTrash && !x.IsDraft &&
                (x.SenderEmail == user.Email || x.ReceiverEmail == user.Email));

            if (message == null)
                return NotFound();

            message.IsTrash = true;
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { id, isTrash = true });

            TempData["SuccessMessage"] = "Mesaj çöp kutusuna taşındı.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Inbox));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMessage(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (id <= 0) return BadRequest();

            var message = await _context.Messages.FirstOrDefaultAsync(x =>
                x.MessageId == id &&
                x.IsTrash && !x.IsDraft &&
                (x.SenderEmail == user.Email || x.ReceiverEmail == user.Email));

            if (message == null)
                return NotFound();

            message.IsTrash = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mesaj geri yüklendi.";
            return RedirectToAction(nameof(Trash));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (id <= 0) return BadRequest();

            var message = await _context.Messages.FirstOrDefaultAsync(x =>
                x.MessageId == id &&
                x.IsTrash && !x.IsDraft &&
                (x.SenderEmail == user.Email || x.ReceiverEmail == user.Email));

            if (message == null)
                return NotFound();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mesaj kalıcı olarak silindi.";
            return RedirectToAction(nameof(Trash));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMessagesRead(List<int> ids)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if(ids== null || !ids.Any()) return BadRequest();
            var messages = await _context.Messages.Where(x=> ids.Contains(x.MessageId)&&x.ReceiverEmail==user.Email && !x.IsDraft && !x.IsTrash).ToListAsync();
            foreach(var message in messages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMessagesUnread(List<int> ids)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (ids == null || !ids.Any())
                return BadRequest();

            var messages = await _context.Messages
                .Where(x =>
                    ids.Contains(x.MessageId) &&
                    x.ReceiverEmail == user.Email &&
                    !x.IsDraft &&
                    !x.IsTrash)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = false;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
