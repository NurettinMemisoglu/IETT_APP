using IETT_APP.Domain.Enums;
using System.ComponentModel.DataAnnotations;

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

        public static string ToDisplayName(this LineType type) => type switch
        {
            LineType.IETT => "İETT",
            LineType.OHO => "ÖHO",
            LineType.METROBUS => "Metrobüs",
            _ => type.ToString()
        };


        public static string ToDisplayName<TEnum>(this TEnum value) where TEnum : Enum
        {
            var displayAttr = value.GetType()
                                   .GetField(value.ToString())
                                   ?.GetCustomAttributes(typeof(DisplayAttribute), false)
                                   .FirstOrDefault() as DisplayAttribute;
            return displayAttr?.Name ?? value.ToString();

        }
    }
}