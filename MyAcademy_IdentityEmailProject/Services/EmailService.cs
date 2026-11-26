
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MyAcademy_IdentityEmailProject.Settings;

namespace MyAcademy_IdentityEmailProject.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendAsync(
            string to,
            string subject,
            string body)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(
                _settings.SenderName,
                _settings.SenderEmail));

            mimeMessage.To.Add(MailboxAddress.Parse(to));

            mimeMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = body
            };

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls);

            await smtpClient.AuthenticateAsync(
                _settings.Username,
                _settings.Password);

            await smtpClient.SendAsync(mimeMessage);

            await smtpClient.DisconnectAsync(true);
        }
    }
}
