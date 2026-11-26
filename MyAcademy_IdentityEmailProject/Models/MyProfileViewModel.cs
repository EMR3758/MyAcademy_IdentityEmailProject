using System.ComponentModel.DataAnnotations;

namespace MyAcademy_IdentityEmailProject.Models
{
    public class MyProfileViewModel
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage ="Geçerli e-posta adresi giriniz.")]
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

    }
}
