using System.Net.Http.Headers;

namespace IETT_APP.WebMVC.Extensions
{
    public static class HttpContentExtensions
    {
        public static MultipartFormDataContent ToMultipartContent(this object dto)
        {
            var content = new MultipartFormDataContent();
            var properties = dto.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(dto);
                if (value == null) continue; // Boşsa geç

                // 1. Eğer özellik bir DOSYA ise
                if (value is IFormFile file)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, prop.Name, file.FileName);
                }
                // 2. Eğer özellik TARİH ise (Formatlı gönderelim)
                else if (value is DateTime dateVal)
                {
                    content.Add(new StringContent(dateVal.ToString("o")), prop.Name); // ISO 8601
                }
                // 3. Diğer her şey (String, Int, Bool vb.)
                else
                {
                    content.Add(new StringContent(value.ToString()!), prop.Name);
                }
            }

            return content;
        }
    }
}