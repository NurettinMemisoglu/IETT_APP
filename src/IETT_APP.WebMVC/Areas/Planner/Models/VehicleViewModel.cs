using IETT_APP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Planner.Models
{
    public class VehicleViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Kapı Numarası")]
        [Required(ErrorMessage = "Kapı numarası zorunludur.")]
        [StringLength(50, ErrorMessage = "Kapı numarası 50 karakteri geçemez.")]
        public string DoorNumber { get; set; } = string.Empty;

        [Display(Name = "Plaka Numarası")]
        [Required(ErrorMessage = "Plaka numarası zorunludur.")]
        [StringLength(20, ErrorMessage = "Plaka numarası 20 karakteri geçemez.")]
        public string PlateNumber { get; set; } = string.Empty;

        [Display(Name = "Kapasite")]
        [Range(1, int.MaxValue, ErrorMessage = "Kapasite pozitif bir değer olmalıdır.")]
        public int Capacity { get; set; }

        [Display(Name = "Garaj")]
        [Required(ErrorMessage = "Garaj seçimi zorunludur.")]
        public Guid GarageId { get; set; }

        [Display(Name = "Servis Durumu")]
        [Required]
        public ServiceStatus ServiceStatus { get; set; }

        [Display(Name = "Operatör")]
        [Required]
        public VehicleType Driver { get; set; }

        [Display(Name = "Model")]
        [Required]
        public VehicleModel Model { get; set; }

        [Display(Name = "Üretim Yılı")]
        [Range(1950, 2100, ErrorMessage = "Geçerli bir yıl giriniz.")]
        public int Year { get; set; }

        [Display(Name = "Toplam Kilometre")]
        [Range(0, int.MaxValue, ErrorMessage = "Kilometre negatif olamaz.")]
        public int TotalKm { get; set; }

        [Display(Name = "Engelli Erişimi Var mı?")]
        public bool HasDisabilityAccess { get; set; } = true; // default true

        [Display(Name = "WiFi Var mı?")]
        public bool HasWiFi { get; set; } = true; // default true

        [Display(Name = "Bisiklet Taşıma Aparatı")]
        public bool HasBikeRack { get; set; } = false; // default false

        [Display(Name = "Şarj Ünitesi")]
        public bool HasMobileCharging { get; set; } = false; // default false

        [Display(Name = "Yolcu Bilgilendirme Sistemi")]
        public bool HasPassengerInfoSystem { get; set; } = false; // default false

        [Display(Name = "Kamera (CCTV)")]
        public bool HasCCTV { get; set; } = true; // default true

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true; // default true

        [Display(Name = "Atandı mı?")]
        public bool IsAssigned { get; set; } = false; // default false


        // Meta Bilgiler
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Son Güncelleme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; }

        public string GarageName { get; set; } = string.Empty;

        // Dropdown verileri (controller’da doldurulacak)
        public SelectList? GarageList { get; set; }
        public SelectList? ServiceStatusList { get; set; }
        public SelectList? OperatorList { get; set; }
        public SelectList? ModelList { get; set; }
    }
}
