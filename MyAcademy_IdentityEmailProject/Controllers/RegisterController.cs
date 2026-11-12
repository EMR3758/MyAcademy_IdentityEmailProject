using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyAcademy_IdentityEmailProject.Entities;
using MyAcademy_IdentityEmailProject.Models;

namespace MyAcademy_IdentityEmailProject.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public RegisterController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegisterViewModel userRegisterViewModel)
        {
            AppUser appUser = new AppUser()
            {
                Name = userRegisterViewModel.Name,
                Surname = userRegisterViewModel.Surname,
                Email = userRegisterViewModel.Email,
                UserName = userRegisterViewModel.Username
            };
            var result = await _userManager.CreateAsync(appUser,userRegisterViewModel.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("SignIn","Login");
            }
            else
            {
                return View();
            }
  
        }
    }
}
