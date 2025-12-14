using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Admin.Models
{
    public class UserUpdateViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Ad")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string Surname { get; set; } = string.Empty;

        [Display(Name = "E-Posta (Kullanıcı Adı)")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        // Not: Rolleri bu modelde göstermiyoruz, onlar ayrı modalda.
        // Ancak listede göstermek için RoleNames lazım olabilir.
        public List<string> RoleNames { get; set; } = new();
    }
}