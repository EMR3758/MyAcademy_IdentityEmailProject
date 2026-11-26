using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyAcademy_IdentityEmailProject.Entities;
using MyAcademy_IdentityEmailProject.Models;
using MyAcademy_IdentityEmailProject.Services;
using System.Security.Cryptography;


namespace MyAcademy_IdentityEmailProject.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(UserManager<AppUser> userManager, IEmailService emailService, ILogger<RegisterController> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(UserRegisterViewModel userRegisterViewModel)
        {
            // new Random() tahmin edilebilir, güvenlik kodunda kriptografik rastgele sayı kullanılır
            int validCode = RandomNumberGenerator.GetInt32(100000, 1000000);
            if (!ModelState.IsValid) 
            {
                return View(userRegisterViewModel);
            }

            AppUser appUser = new AppUser()
            {
                Name = userRegisterViewModel.Name,
                Surname = userRegisterViewModel.Surname,
                Email = userRegisterViewModel.Email,
                UserName = userRegisterViewModel.Username,
                ValidCode = validCode,
                VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(10)
            };
            var result = await _userManager.CreateAsync(appUser,userRegisterViewModel.Password);
            if (result.Succeeded)
            {
                try
                {
                    await _emailService.SendAsync(
                        userRegisterViewModel.Email,
                        "Email Doğrulama Kodu",
                        "Sisteme giriş yapabilmek için gerekli olan doğrulama kodu: " + validCode);
                }
                catch (Exception ex)
                {
                   
                    _logger.LogError(ex, "Doğrulama e-postası gönderilemedi. Kullanıcı: {UserName}", appUser.UserName);

                    var deleteResult = await _userManager.DeleteAsync(appUser);
                    if (!deleteResult.Succeeded)
                    {
                        _logger.LogError("E-posta hatası sonrası kullanıcı silinemedi: {UserName} - {Errors}",
                            appUser.UserName,
                            string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                    }

                    ModelState.AddModelError(string.Empty,
                        "Doğrulama e-postası gönderilemedi. Lütfen birkaç dakika sonra tekrar deneyin.");

                    return View(userRegisterViewModel);
                }

                return RedirectToAction("VerifyEmail", new {email=appUser.Email});

            }
            foreach (var error in result.Errors) 
            { 
                ModelState.AddModelError(string.Empty, error.Description);            
            }

            return View(userRegisterViewModel);
  
        }
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("SignUp");
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.EmailConfirmed) 
            {
                return RedirectToAction("SignUp");
            }
            var model = new VerifyEmailViewModel { Email = email};
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty,
                    "Kullanıcı bulunamadı veya e-posta zaten doğrulanmış.");

                return View(model);
            }

            if (user.ValidCode == null ||
                user.ValidCode.Value.ToString() != model.Code)
            {
                ModelState.AddModelError(nameof(model.Code),
                    "Doğrulama kodu hatalı.");

                return View(model);
            }

            if (user.VerificationCodeExpiry == null ||
                DateTime.UtcNow > user.VerificationCodeExpiry.Value)
            {
                ModelState.AddModelError(string.Empty,
                    "Doğrulama kodunun süresi dolmuş. Yeni kod talep ediniz.");

                return View(model);
            }

            user.EmailConfirmed = true;

            user.ValidCode = null;
            user.VerificationCodeExpiry = null;


            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            TempData["SuccessMessage"] =
                "E-posta adresiniz başarıyla doğrulandı. Giriş yapabilirsiniz.";

            return RedirectToAction("SignIn", "Login");
        }

    }
}
