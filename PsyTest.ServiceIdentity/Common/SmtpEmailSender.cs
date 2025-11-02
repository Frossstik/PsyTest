using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace PsyTest.ServiceIdentity.Common
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = _config.GetSection("Smtp");
            var host = smtp["Host"];
            var port = int.Parse(smtp["Port"] ?? "465");
            var enableSsl = bool.Parse(smtp["EnableSsl"] ?? "true");
            var user = smtp["User"];
            var pass = smtp["Password"];
            var senderName = smtp["SenderName"] ?? "PsyTest";
            var senderEmail = smtp["SenderEmail"] ?? user;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

            using var client = new SmtpClient();

            // Mail.ru не поддерживает XOAUTH2
            client.AuthenticationMechanisms.Remove("XOAUTH2");

            // 🔹 Определяем режим TLS
            SecureSocketOptions socketOptions;
            if (port == 465)
                socketOptions = SecureSocketOptions.SslOnConnect; // SSL сразу
            else if (port == 587)
                socketOptions = SecureSocketOptions.StartTls;     // сначала plain -> STARTTLS
            else
                socketOptions = enableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;

            try
            {
                // Иногда помогает для самоподписанных сертификатов в dev
                // client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(host, port, socketOptions);
                await client.AuthenticateAsync(user, pass);
                await client.SendAsync(message);

                Console.WriteLine($"✅ Email sent to {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error sending email: {ex.Message}");
                throw;
            }
            finally
            {
                try { await client.DisconnectAsync(true); } catch { }
            }
        }
    }
}
