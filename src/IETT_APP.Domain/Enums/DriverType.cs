using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum DriverType
    {
        [Display(Name = "İETT Kadrolu")]
        IETT_Staff = 0,

        [Display(Name = "Özel Halk Otobüsü")]
        OHO_Driver = 1,

        [Display(Name = "Taşeron")]
        Subcontractor = 2,

        [Display(Name = "AŞ Personeli")]
        AS_Staff = 3
    }
}