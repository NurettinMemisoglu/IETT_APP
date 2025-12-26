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
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        // --- YENİ EKLENEN KISIM ---
        [Display(Name = "Telefon Numarası")]
        [Required(ErrorMessage = "{0} alanı zorunludur.")]
        // Regex: Başında 0 olmadan, 1-9 ile başlayan 10 haneli sayı
        [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Telefon numarası başında '0' olmadan, 10 haneli girilmelidir. (Örn: 542xxxxxxx)")]
        public string PhoneNumber { get; set; } = string.Empty;
        // --------------------------

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