using IETT_APP.Application.Interfaces;
using MailKit.Net.Smtp; // MailKit'in Smtp istemcisi (System.Net.Mail DEĞİL)
using Microsoft.Extensions.Configuration;
using MimeKit; // MailKit'in mesaj yapısı

namespace IETT_APP.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var smtpSettings = _config.GetSection("EmailSettings");

            // 1. Mesajı Oluştur (MimeMessage kullanıyoruz)
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(smtpSettings["SenderName"], smtpSettings["SenderEmail"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            if (isHtml) bodyBuilder.HtmlBody = body;
            else bodyBuilder.TextBody = body;

            email.Body = bodyBuilder.ToMessageBody();

            // 2. SMTP Bağlantısı (MailKit)
            using (var client = new SmtpClient())
            {
                client.Timeout = 20000;
                try
                {
                    // SSL Sertifika hatasını yoksay (Development ortamı için)
                    // Gerçek sunucuda bunu kaldırabilirsin.
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    // Bağlan
                    // SecureSocketOptions.StartTls: Modern sunucular için (Port 587)
                    // SecureSocketOptions.Auto: Otomatik algıla (Port 25 veya 2525 için)
                    await client.ConnectAsync(
                        smtpSettings["SmtpServer"],
                        int.Parse(smtpSettings["Port"]!),
                        MailKit.Security.SecureSocketOptions.Auto); // MailTrap için StartTls genelde iyidir

                    // Giriş Yap
                    await client.AuthenticateAsync(smtpSettings["Username"], smtpSettings["Password"]);

                    // Gönder
                    await client.SendAsync(email);
                }
                catch (Exception ex)
                {
                    // Hata detayını görmek için fırlatıyoruz (TripTaskService yakalayacak)
                    throw new Exception($"MailKit Bağlantı Hatası: {ex.Message}", ex);
                }
                finally
                {
                    // Bağlantıyı temiz bir şekilde kes
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}