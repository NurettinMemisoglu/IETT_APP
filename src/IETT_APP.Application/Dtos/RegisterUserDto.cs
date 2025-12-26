using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "İsim alanı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyisim alanı zorunludur.")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        // REGEX AÇIKLAMASI:
        // ^      -> Başlangıç
        // [1-9]  -> İlk karakter 0 OLAMAZ (1-9 arası olmalı)
        // [0-9]{9} -> Devamında tam 9 tane rakam gelmeli (Toplam 10 hane)
        // $      -> Bitiş
        [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Telefon numarası başında '0' olmadan, 10 haneli girilmelidir. (Örn: 5422189430)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}