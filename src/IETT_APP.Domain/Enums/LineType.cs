using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum LineType
    {
        [Display(Name = "İETT")]
        IETT = 1,
        [Display(Name = "ÖHO")]
        OHO = 2,
        [Display(Name = "Metrobüs")]
        METROBUS = 3,
    }
}
