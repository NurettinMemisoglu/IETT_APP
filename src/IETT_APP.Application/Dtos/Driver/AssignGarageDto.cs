using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Application.Dtos.Driver
{
    public class AssignGarageDto
    {
        [Required]
        public Guid DriverId { get; set; }

        [Required]
        public Guid GarageId { get; set; }
    }
}