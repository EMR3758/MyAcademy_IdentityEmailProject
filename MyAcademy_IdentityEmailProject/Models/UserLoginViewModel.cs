using System.ComponentModel.DataAnnotations;

namespace MyAcademy_IdentityEmailProject.Models
{
    public class UserLoginViewModel
    {
        [Required(ErrorMessage ="Kullanıcı adı zorunludur.")]
        public string Username { get; set; }

        [Required(ErrorMessage ="Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
