using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum VehicleOperator
    {
        [Display(Name = "İETT")]
        IETT = 1,
        [Display(Name = "ÖHO")]
        OHO = 2
    }
}
