// IETT_APP.WebUI/Areas/Planner/Models/AssignGarageViewModel.cs

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Planner.Models
{
    public class AssignGarageViewModel
    {
        [Required]
        public Guid DriverId { get; set; }

        // Kullanıcıya kimin için işlem yaptığını göstermek için (Sadece okuma amaçlı)
        public string DriverFullName { get; set; }

        [Required(ErrorMessage = "Lütfen bir garaj seçiniz.")]
        [Display(Name = "Atanacak Garaj")]
        public Guid GarageId { get; set; }

        // Dropdown'ı doldurmak için gerekli liste
        public SelectList? GarageList { get; set; }
    }
}