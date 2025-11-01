using System.ComponentModel.DataAnnotations;

namespace IETT_APP.Domain.Enums
{
    public enum RoutesDirection
    {
        [Display(Name = "Gidiş")]
        Outbound = 1,
        [Display(Name = "Dönüş")]
        Inbound = 2
    }
}
