using IETT_APP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Chief.Models
{
    public class TripTaskViewModel
    {
        // === Temel Bilgiler ===
        public Guid Id { get; set; }

        [Display(Name = "Görev Başlığı")]
        [Required(ErrorMessage = "Başlık zorunludur.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Durum")]
        public TaskState Status { get; set; }

        [Display(Name = "Durum Nedeni")]
        public string? StatusReason { get; set; }

        // === Sayısal Veriler ===
        [Display(Name = "Yolcu Sayısı")]
        [Range(0, int.MaxValue, ErrorMessage = "Pozitif bir değer giriniz.")]
        public int? PassengerCount { get; set; }

        [Display(Name = "Gecikme (İniş)")]
        public int? DelayInMinutes { get; set; }

        [Display(Name = "Gecikme (Çıkış)")]
        public int? DelayOutMinutes { get; set; }

        // === Zaman Bilgileri ===
        [Display(Name = "Planlanan Kalkış")]
        public DateTime? ScheduledDeparture { get; set; }

        [Display(Name = "Planlanan Varış")]
        public DateTime? ScheduledArrival { get; set; }

        [Display(Name = "Ayarlanan Kalkış")]
        public DateTime? AdjustedDeparture { get; set; }

        [Display(Name = "Ayarlanan Varış")]
        public DateTime? AdjustedArrival { get; set; }

        [Display(Name = "Gerçek Kalkış")]
        public DateTime? ActualDeparture { get; set; }

        [Display(Name = "Gerçek Varış")]
        public DateTime? ActualArrival { get; set; }

        // === Foreign Keys ===
        [Display(Name = "Araç")]
        public Guid? VehicleId { get; set; }

        [Display(Name = "Operatör")]
        public Guid? OperatorId { get; set; }

        [Display(Name = "Hat")]
        public Guid? LineId { get; set; }

        [Display(Name = "Güzergah")]
        public Guid? RouteId { get; set; }

        [Display(Name = "Garaj")]
        public Guid? GarageId { get; set; }

        // === Audit ===
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // === UI için okunabilir alanlar ===
        public string? VehicleName { get; set; }
        public string? OperatorName { get; set; }
        public string? LineName { get; set; }
        public string? RouteName { get; set; }
        public string? GarageName { get; set; }

        // === Dropdown listeler ===
        public SelectList? VehicleList { get; set; }
        public SelectList? OperatorList { get; set; }
        public SelectList? LineList { get; set; }
        public SelectList? RouteList { get; set; }
        public SelectList? GarageList { get; set; }
        public SelectList? TaskStateList { get; set; }
    }
}
