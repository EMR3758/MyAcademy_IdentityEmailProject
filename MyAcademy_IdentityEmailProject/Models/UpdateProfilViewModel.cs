using System.ComponentModel.DataAnnotations;

namespace MyAcademy_IdentityEmailProject.Models
{
    public class UpdateProfilViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string Surname { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Description { get; set; }
    }
}
