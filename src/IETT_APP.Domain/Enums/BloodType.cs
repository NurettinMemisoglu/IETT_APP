using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum BloodType
    {
        [Display(Name = "A Rh+")]
        ARhPositive = 1,

        [Display(Name = "A Rh-")]
        ARhNegative = 2,

        [Display(Name = "B Rh+")]
        BRhPositive = 3,

        [Display(Name = "B Rh-")]
        BRhNegative = 4,

        [Display(Name = "AB Rh+")]
        ABRhPositive = 5,

        [Display(Name = "AB Rh-")]
        ABRhNegative = 6,

        [Display(Name = "0 Rh+")]
        ORhPositive = 7,

        [Display(Name = "0 Rh-")]
        ORhNegative = 8
    }
}