using IETT_APP.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Driver.Models
{
    public class DriverTripTaskViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Görev Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Açıklama / Notlar")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Durum")]
        public TaskState Status { get; set; }

        // --- ZAMANLAMA ---
        [Display(Name = "Planlanan Kalkış")]
        public DateTime? ScheduledDeparture { get; set; }

        [Display(Name = "Planlanan Varış")]
        public DateTime? ScheduledArrival { get; set; }

        [Display(Name = "Revize Kalkış")]
        public DateTime? AdjustedDeparture { get; set; } // Şoför için önemli

        [Display(Name = "Gerçekleşen Kalkış")]
        public DateTime? ActualDeparture { get; set; }

        [Display(Name = "Gerçekleşen Varış")]
        public DateTime? ActualArrival { get; set; }

        // --- KAYNAKLAR (Sadece Okunabilir) ---
        [Display(Name = "Araç")]
        public string? VehicleName { get; set; } // Plaka

        [Display(Name = "Hat")]
        public string? LineName { get; set; }

        [Display(Name = "Güzergah")]
        public string? RouteName { get; set; }

        [Display(Name = "Başlangıç Garajı")]
        public string? GarageName { get; set; }

        // --- OPERASYONEL GİRİŞLER (Modal İçin) ---
        // Bu alanlar sadece veri girişi sırasında (POST) kullanılacak
        [Display(Name = "Yolcu Sayısı")]
        public int InputPassengerCount { get; set; }

        [Display(Name = "Bitiş Kilometresi")]
        public decimal InputEndOdometer { get; set; }

        [Display(Name = "Sorun / İptal Nedeni")]
        public string InputReason { get; set; } = string.Empty;
    }
}