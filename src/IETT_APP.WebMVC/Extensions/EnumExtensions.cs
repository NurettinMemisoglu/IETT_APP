using IETT_APP.Domain.Enums;

namespace IETT_APP.WebMVC.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDisplayName(this StopType type) => type switch
        {
            StopType.AcikDurak => "Açık Durak",
            StopType.KapaliDurak => "Kapalı Durak",
            StopType.FullKapaliDurak => "Full Kapalı Durak",
            _ => type.ToString()
        };

        public static string ToDisplayName(this SmartStop type) => type switch
        {
            SmartStop.Yes => "Evet",
            SmartStop.No => "Hayır",
            _ => type.ToString()
        };
    }
}