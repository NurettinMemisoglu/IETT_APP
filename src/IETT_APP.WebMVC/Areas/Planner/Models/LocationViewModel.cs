using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Planner.Models
{
    public class LocationViewModel
    {
        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }
    }
}
