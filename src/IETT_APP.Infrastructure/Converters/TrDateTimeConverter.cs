using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IETT_APP.Infrastructure.Converters // Namespace'i kendi projene göre ayarla
{
    public class TrDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string Format = "dd.MM.yyyy HH:mm";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();

            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            // Gelen string'i özel formata göre parse etmeyi dene
            if (DateTime.TryParseExact(dateString, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            // Eğer format uymazsa standart parse dene (Fallback)
            if (DateTime.TryParse(dateString, out var standardDate))
            {
                return standardDate;
            }

            return null; // Hiçbiri olmazsa null dön
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(Format));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}