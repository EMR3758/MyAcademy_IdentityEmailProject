using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAcademy_IdentityEmailProject.Context;
using MyAcademy_IdentityEmailProject.Entities;
using MyAcademy_IdentityEmailProject.Models;

namespace MyAcademy_IdentityEmailProject.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var email = user.Email;
            var today = DateTime.Today;
            var from = today.AddDays(-29);
            var weekStart = today.AddDays(-6);

            var inbox = _context.Messages.Where(x => x.ReceiverEmail == email && !x.IsDraft && !x.IsTrash);
            var sent = _context.Messages.Where(x => x.SenderEmail == email && !x.IsDraft && !x.IsTrash);

            var incomingDates = await inbox.Where(x => x.SendDate >= from).Select(x => x.SendDate).ToListAsync();
            var outgoingDates = await sent.Where(x => x.SendDate >= from).Select(x => x.SendDate).ToListAsync();

            var incomingByDay = incomingDates.GroupBy(d => d.Date).ToDictionary(g => g.Key, g => g.Count());
            var outgoingByDay = outgoingDates.GroupBy(d => d.Date).ToDictionary(g => g.Key, g => g.Count());

            var model = new DashboardViewModel
            {
                UserName = user.Name ?? user.UserName ?? "Kullanıcı",
                UserSurname = user.Surname ?? "",
                UserEmail = email ?? "",
                UserImage = user.ImageUrl,
                EmailConfirmed = user.EmailConfirmed,

                TotalInbox = await inbox.CountAsync(),
                TotalSent = await sent.CountAsync(),
                UnreadCount = await inbox.CountAsync(x => !x.IsRead),

                FavoriteCount = await _context.MessageFavorites.CountAsync(x => x.UserId == user.Id),

                DraftCount = await _context.Messages.CountAsync(x => x.SenderEmail == email && x.IsDraft),

                TrashCount = await _context.Messages.CountAsync(x =>
                    (x.SenderEmail == email || x.ReceiverEmail == email) &&
                    x.IsTrash && !x.IsDraft),

                RecentMessages = await inbox.OrderByDescending(x => x.SendDate).Take(5).ToListAsync(),
                RecentSentMessages = await sent.OrderByDescending(x => x.SendDate).Take(5).ToListAsync(),

                DailyTraffic = Enumerable.Range(0, 30)
                    .Select(i => from.AddDays(i))
                    .Select(day => new DailyTrafficItem
                    {
                        Date = day,
                        Incoming = incomingByDay.GetValueOrDefault(day),
                        Outgoing = outgoingByDay.GetValueOrDefault(day)
                    })
                    .ToList(),

                WeeklySentCount = outgoingDates.Count(d => d >= weekStart),

                CategoryCounts = await _context.Messages
                    .Where(x =>
                        (x.SenderEmail == email || x.ReceiverEmail == email) &&
                        !x.IsDraft && !x.IsTrash)
                    .GroupBy(x => new { x.CategoryId, x.Category.CategoryName })
                    .Select(g => new CategoryCountItem
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.CategoryName,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync()
            };

            return View("Dashboard", model);
        }
    }
}
