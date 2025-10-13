using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Areas.Planner.Models
{
    public class StopViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        // Keep as string since your DTOs/controllers expect Type as string (enum name)
        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public LocationViewModel Location { get; set; } = new LocationViewModel();
    }
}
