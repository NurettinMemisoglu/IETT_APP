using IETT_APP.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Planner.Models
{
    public class LineViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Hat Kodu")]
        [Required(ErrorMessage = "Hat kodu zorunludur.")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Display(Name = "Hat Adı")]
        [Required(ErrorMessage = "Hat adı zorunludur.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Hat Tipi")]
        [Required(ErrorMessage = "Hat tipi zorunludur.")]
        public LineType LineType { get; set; }

        [Display(Name = "Araç Sayısı")]
        public int VehicleCount { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Son Düzenleme Tarihi")]
        public DateTime? UpdatedAt { get; set; }


        public SelectList? LineTypeList { get; set; }
    }
}

