using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyAcademy_IdentityEmailProject.Entities;

namespace MyAcademy_IdentityEmailProject.ViewComponents.MessageLayoutViewComponents
{
    public class _MessageLayoutNavbarComponentPartial:ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;

        public _MessageLayoutNavbarComponentPartial(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return View(user);
        }
    }
}
