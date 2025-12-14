using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum ServiceStatus
    {
        [Display(Name = "Servise Hazır")]
        InService = 1,

        [Display(Name = "Servis Dışı")]
        OutOfService = 2,

        [Display(Name = "Bakımda")]
        UnderMaintenance = 3,

        [Display(Name = "Hasarlı")]
        Damaged = 4,

        [Display(Name = "Görev dışı")]
        OutOfDuty = 5,

        [Display(Name = "Seferde / Yolda")]
        OnRoute = 6
    }
}