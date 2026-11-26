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

            // Yalnızca giriş yapan kullanıcıya gelen mesajlar (taslak ve çöp hariç)
            var values = await _context.Messages
                .Where(x => x.ReceiverEmail == user.Email && !x.IsDraft && !x.IsTrash)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            // Kullanıcının kendi yıldızladığı mesajlar (MessageFavorites, kullanıcıya özel)
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

            // Mesaja yalnızca göndereni veya alıcısı erişebilir; başkasınınki için 404 döner
            var message = await _context.Messages
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.MessageId == id &&
                                          (x.SenderEmail == user.Email || x.ReceiverEmail == user.Email));
            if (message == null)
                return NotFound();

            // Alıcı açtığında okunmamış mesaj okundu olarak işaretlenir (gönderenin açması durumu değiştirmez)
            if (!message.IsRead && message.ReceiverEmail == user.Email)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            ViewBag.IsFavorite = await _context.MessageFavorites
                .AnyAsync(x => x.MessageId == id && x.UserId == user.Id);

            return View(message);
        }

        public IActionResult CreateMessage()
        {
            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "CategoryId", "CategoryName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMessage(Message message)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Challenge();

            message.SenderEmail = user.Email;
            message.SendDate = DateTime.Now;
            message.IsRead = false;
            ModelState.Remove("SenderEmail");
            ModelState.Remove("Category");
            message.IsFavorite = false;
            message.IsDraft = false;
            message.IsTrash = false;

            if (string.IsNullOrWhiteSpace(message.ReceiverEmail) ||
       !Regex.IsMatch(message.ReceiverEmail,@"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("", "Geçerli bir alıcı e-posta adresi giriniz.");
            }

            if (string.IsNullOrWhiteSpace(message.Subject))
                ModelState.AddModelError("", "Konu alanı boş bırakılamaz.");

            if (string.IsNullOrWhiteSpace(message.MessageDetail))
                ModelState.AddModelError("", "Mesaj içeriği boş bırakılamaz.");

            var categoryExists = await _context.Categories.AnyAsync(x => x.CategoryId == message.CategoryId);
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
                    var plainText = WebUtility.HtmlDecode(Regex.Replace(message.MessageDetail, "<.*?>", " "));
                    await _emailService.SendAsync(message.ReceiverEmail,message.Subject, plainText);
                    _context.Messages.Add(message);
                    await _context.SaveChangesAsync();
                }
                TempData["SuccessMessage"] = "Mesaj başarıyla gönderildi.";
                return RedirectToAction(nameof(SendBox));
            }
            catch(Exception) 
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
            var values = _context.Messages
                .Where(x => x.SenderEmail == user.Email)
                .OrderByDescending(x => x.SendDate)
                .ToList();

            return View(values);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id, string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();
            var message = await _context.Messages.FirstOrDefaultAsync(x => x.MessageId == id && (x.SenderEmail == user.Email || x.ReceiverEmail == user.Email));
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

            // Gelen Kutusu'ndaki yıldız butonu fetch ile çağırır → sayfa yönlendirmesi yerine yeni durumu döndür
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
            var values = await _context.MessageFavorites.Where(x => x.UserId == user.Id && (x.Message.SenderEmail == user.Email || x.Message.ReceiverEmail == user.Email)).OrderByDescending(x => x.Message.SendDate).Select(x => x.Message).ToListAsync();
            return View(values);
        }

    }
}
