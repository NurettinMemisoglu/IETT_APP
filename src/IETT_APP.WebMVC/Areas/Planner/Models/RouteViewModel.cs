using IETT_APP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Planner.Models
{
    public class RouteViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Güzergah Kodu")]
        [Required(ErrorMessage = "Route kodu zorunludur.")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Güzergah Adı")]
        [Required(ErrorMessage = "Route adı zorunludur.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mesafe (m)")]
        [Range(0, int.MaxValue, ErrorMessage = "Mesafe pozitif olmalıdır.")]
        public int LengthInM { get; set; }

        [Display(Name = "Süre (dk)")]
        [Range(0, int.MaxValue, ErrorMessage = "Süre pozitif olmalıdır.")]
        public int TimeInMinutes { get; set; }

        [Display(Name = "Gidiş Yönü")]
        [Required(ErrorMessage = "Gidiş yönü zorunludur.")]
        public RoutesDirection RoutesDirection { get; set; }

        [Display(Name = "Hat Seçimi")]
        [Required(ErrorMessage = "Hat seçimi zorunludur.")]
        public Guid LineId { get; set; }

        [Display(Name = "Duraklar")]
        public List<Guid> StopIds { get; set; } = new();

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Silindi mi?")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Son Düzenleme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Silinme Tarihi")]
        public DateTime? DeletedAt { get; set; }

        // Gösterim amaçlı, backend’den async çekilecek
        public string LineName { get; set; } = string.Empty;

        public List<string>? StopNames { get; set; }
        // Dropdown listeler
        public SelectList? RoutesDirectionList { get; set; }
        public SelectList? LineList { get; set; }
        public SelectList? StopList { get; set; }


    }
}
