using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.TripTask
{
    public class FailTripRequestDto
    {
        // Arıza veya kaza nedeni zorunludur
        [Required(ErrorMessage = "Arıza/Sorun nedeni belirtilmelidir.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Açıklama en az 5 karakter olmalıdır.")]
        public string Reason { get; set; } = string.Empty;
    }
}