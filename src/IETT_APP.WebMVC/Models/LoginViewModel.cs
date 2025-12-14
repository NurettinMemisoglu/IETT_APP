using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Models
{
    public class LoginViewModel
    {
        [Display(Name = "E-Posta Adresi")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Şifre")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}