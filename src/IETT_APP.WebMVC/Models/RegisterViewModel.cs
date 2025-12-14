using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Models
{
    public class RegisterViewModel
    {
        [Display(Name = "Ad")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        public string Surname { get; set; } = string.Empty;

        [Display(Name = "E-Posta Adresi")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Şifre")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter olmalıdır.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Şifre Tekrar")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler birbiriyle uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}