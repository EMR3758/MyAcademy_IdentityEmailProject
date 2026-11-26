using System.ComponentModel.DataAnnotations;

namespace MyAcademy_IdentityEmailProject.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mevcut şifrenizi giriniz.")]
        [StringLength(100,MinimumLength = 6,ErrorMessage ="Yeni şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mevcut şifrenizi giriniz.")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Yeni şifreler birbiriyle eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;


    }
}
