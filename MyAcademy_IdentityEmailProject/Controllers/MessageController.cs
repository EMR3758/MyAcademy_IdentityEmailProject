using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAcademy_IdentityEmailProject.Context;
using MyAcademy_IdentityEmailProject.Entities;


namespace MYAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;
        public MessageController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        public IActionResult CreateMessage(Message message)
        {
            return RedirectToAction("Sendbox");
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
        public async Task<IActionResult> ToggleFavorite(int id,string? returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();
            var message = await _context.Messages.FirstOrDefaultAsync(x=>x.MessageId == id && (x.SenderEmail == user.Email || x.ReceiverEmail == user.Email));
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
            var values = await _context.MessageFavorites.Where(x=>x.UserId == user.Id && (x.Message.SenderEmail == user.Email || x.Message.ReceiverEmail == user.Email)).OrderByDescending(x=>x.Message.SendDate).Select(x=>x.Message).ToListAsync();
            return View(values);
        }

    }
}
