using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.TripTask
{
    public class CompleteTripRequestDto
    {
        [Required(ErrorMessage = "Yolcu sayısı girilmesi zorunludur.")]
        [Range(0, 2000, ErrorMessage = "Yolcu sayısı 0 ile 2000 arasında olmalıdır.")]
        public int PassengerCount { get; set; }

        [Required(ErrorMessage = "Bitiş kilometresi zorunludur.")]
        [Range(0, double.MaxValue, ErrorMessage = "Geçerli bir kilometre giriniz.")]
        public decimal EndOdometerInput { get; set; }

        [Range(0, 100, ErrorMessage = "Yakıt yüzdesi 0-100 arasında olmalıdır.")]
        public int? FuelLevel { get; set; }

        public string? DriverNotes { get; set; }
    }
}