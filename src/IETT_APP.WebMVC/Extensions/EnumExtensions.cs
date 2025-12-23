using IETT_APP.Domain.Enums;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace IETT_APP.WebMVC.Extensions
{
    public static class EnumExtensions
    {
        // Okunan değerleri hafızada tutacak bir sözlük (Cache)
        private static readonly ConcurrentDictionary<Enum, string> _cache = new();

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

        public static string ToDisplayName(this RoutesDirection type) => type switch
        {
            RoutesDirection.Outbound => "Gidiş",
            RoutesDirection.Inbound => "Dönüş",
            _ => type.ToString()
        };




        public static string ToDisplayName(this Enum value)
        {
            // Önce cache'e bakar, varsa oradan getirir. Yoksa hesaplayıp cache'e ekler.
            return _cache.GetOrAdd(value, (enumValue) =>
            {
                var field = enumValue.GetType().GetField(enumValue.ToString());

                var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
                                     .FirstOrDefault() as DisplayAttribute;

                return attribute?.Name ?? enumValue.ToString();
            });
        }
    }
}