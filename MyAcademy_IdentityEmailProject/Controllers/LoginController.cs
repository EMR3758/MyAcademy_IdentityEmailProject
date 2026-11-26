
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyAcademy_IdentityEmailProject.Entities;
using MyAcademy_IdentityEmailProject.Models;

namespace MyAcademy_IdentityEmailProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(UserLoginViewModel userLoginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(userLoginViewModel);
            }
            var user = await _userManager.FindByNameAsync(userLoginViewModel.Username);
            if (user == null) 
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı");
            }
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Giriş yapabilmek için önce e-posta adresinizi doğrulamalısınız.");

                return View(userLoginViewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(userLoginViewModel.Username, userLoginViewModel.Password, false, false);
            if (result.Succeeded) 
            {
                return RedirectToAction("Inbox", "Message");
            }
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı");
            return View(userLoginViewModel);
        }
    }
}
