using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.TripTask
{
    public class RejectTripRequestDto
    {
        // Reddetme nedeni zorunludur
        [Required(ErrorMessage = "Reddetme nedeni belirtilmelidir.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Neden en az 5 karakter olmalıdır.")]
        public string Reason { get; set; } = string.Empty;
    }
}