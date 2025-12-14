namespace IETT_APP.Application.Interfaces
{
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Verilen şablon ismini okur ve içindeki placeholder'ları ({{Key}}) verilen model ile değiştirir.
        /// </summary>
        Task<string> GenerateEmailBodyAsync(string templateName, Dictionary<string, string> placeholders);
    }
}