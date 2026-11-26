using System.ComponentModel.DataAnnotations;

namespace MyAcademy_IdentityEmailProject.Models
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğrulama kodu giriniz.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Doğrulama kodu 6 haneli olmalıdır.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Doğrulama kodu yalnızca rakamlardan oluşmalıdır. ")]
        public string Code { get; set; } = string.Empty;
    }
}
