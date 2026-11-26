using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyAcademy_IdentityEmailProject.Entities;
using MyAcademy_IdentityEmailProject.Models;
using Microsoft.AspNetCore.Hosting;

namespace MyAcademy_IdentityEmailProject.Controllers
{

    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private const long MaxPhotoSize = 2 * 1024 * 1024;
        private static readonly string[] AllowedPhotoExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public ProfileController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            var model = new MyProfileViewModel
            {
                Name = user.Name,
                Surname = user.Surname,
                Username = user.UserName,
                Email = user.Email,
                Description = user.Description,
                ImageUrl = user.ImageUrl
            };
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfilViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            model.Name = model.Name?.Trim() ?? string.Empty;
            model.Surname = model.Surname?.Trim() ?? string.Empty;
            model.Description = model.Description?.Trim();

            if (!ModelState.IsValid)
            {
                var profileModel = new MyProfileViewModel
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Username = user.UserName,
                    Email = user.Email,
                    Description = model.Description,
                    ImageUrl = user.ImageUrl
                };

                return View(nameof(MyProfile), profileModel);
            }

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Description = model.Description;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] =
                    "Profil bilgileriniz başarıyla güncellendi.";

                return RedirectToAction(nameof(MyProfile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var errorModel = new MyProfileViewModel
            {
                Name = model.Name,
                Surname = model.Surname,
                Username = user.UserName,
                Email = user.Email,
                Description = model.Description,
                ImageUrl = user.ImageUrl
            };

            return View(nameof(MyProfile), errorModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePhoto(ProfilePhotoViewModel model)
        {
            // 1. Kullanıcının fotoğraf seçip seçmediğini kontrol et
            if (model.Photo == null || model.Photo.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir fotoğraf seçiniz.";
                return RedirectToAction(nameof(MyProfile));
            }

            // 2. Dosya boyutunu kontrol et
            if (model.Photo.Length > MaxPhotoSize)
            {
                TempData["ErrorMessage"] = "Fotoğraf boyutu en fazla 2 MB olabilir.";
                return RedirectToAction(nameof(MyProfile));
            }

            // 3. Dosya uzantısını kontrol et
            var extension = Path.GetExtension(model.Photo.FileName).ToLowerInvariant();

            if (!AllowedPhotoExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Yalnızca JPG, JPEG, PNG ve WEBP formatları yüklenebilir.";
                return RedirectToAction(nameof(MyProfile));
            }

            // 4. Dosyanın gerçek fotoğraf olup olmadığını kontrol et
            var header = new byte[12];

            using (var stream = model.Photo.OpenReadStream())
            {
                var bytesRead = await stream.ReadAsync(header, 0, header.Length);

                if (bytesRead < header.Length || !IsValidImageSignature(header))
                {
                    TempData["ErrorMessage"] = "Geçerli bir fotoğraf yükleyiniz.";
                    return RedirectToAction(nameof(MyProfile));
                }
            }

            // 5. Giriş yapan kullanıcıyı bul
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            // 6. Fotoğrafın kaydedileceği klasörü belirle
            var uploadFolder = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "profile"
            );

            // Klasör yoksa oluştur
            Directory.CreateDirectory(uploadFolder);

            // 7. Benzersiz bir dosya adı oluştur
            var fileName = Guid.NewGuid().ToString() + extension;

            // Fotoğrafın tam kayıt yolunu belirle
            var filePath = Path.Combine(uploadFolder, fileName);

            // 8. Fotoğrafı klasöre kaydet
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.Photo.CopyToAsync(fileStream);
            }

            // 9. Kullanıcının profil fotoğrafı adresini güncelle
            var oldImageUrl = user.ImageUrl;
            user.ImageUrl = "/uploads/profile/" + fileName;

            // 10. Değişikliği veritabanına kaydet
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil fotoğrafınız başarıyla güncellendi.";
                return RedirectToAction(nameof(MyProfile));
            }

            // Veritabanı güncellenemezse yeni yüklenen dosyayı sil
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            user.ImageUrl = oldImageUrl;

            TempData["ErrorMessage"] = "Profil fotoğrafı güncellenirken bir hata oluştu.";

            return RedirectToAction(nameof(MyProfile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen şifre bilgilerini kontrol ediniz.";
                return RedirectToAction(nameof(MyProfile));
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                // Yalnızca başarılı değişiklikte: Türkiye saatine göre işlem anı (dd.MM.yyyy HH:mm)
                TempData["PasswordSuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                TempData["PasswordChangedAt"] = GetTurkeyNow().ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                return RedirectToAction(nameof(MyProfile));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            TempData["ErrorMessage"] = "Şifre değiştirilemedi. Mevcut şifrenizi kontrol ediniz.";

            return RedirectToAction(nameof(MyProfile));
        }

        // Sunucunun saat diliminden bağımsız olarak Türkiye yerel saatini döndürür.
        // "Europe/Istanbul" (IANA) Linux/macOS ve ICU'lu Windows'ta, "Turkey Standard Time" eski Windows'ta geçerlidir.
        private static DateTime GetTurkeyNow()
        {
            TimeZoneInfo turkeyZone;
            try
            {
                turkeyZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
            }
            catch (TimeZoneNotFoundException)
            {
                turkeyZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, turkeyZone);
        }

        private static bool IsValidImageSignature(byte[] header)
        {
            // JPEG
            if (header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF)
                return true;

            // PNG
            if (header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A)
                return true;

            // WEBP
            if (header[0] == 0x52 &&
                header[1] == 0x49 &&
                header[2] == 0x46 &&
                header[3] == 0x46 &&
                header[8] == 0x57 &&
                header[9] == 0x45 &&
                header[10] == 0x42 &&
                header[11] == 0x50)
                return true;

            return false;
        }
    }
}
