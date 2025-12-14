using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;

        // ConfirmPassword'e API tarafında gerek yok, o MVC/UI validasyon işidir.
    }
}