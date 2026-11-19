using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace eMotion.Services
{
    /// <summary>
    /// Sends e-mails through SMTP. Credentials are read from configuration
    /// (section "EmailSettings") and must never be committed to source control.
    /// For local development use: dotnet user-secrets set "EmailSettings:Password" "..."
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var settings = _configuration.GetSection("EmailSettings");

            var host = settings["SmtpHost"] ?? "smtp.gmail.com";
            var port = int.TryParse(settings["SmtpPort"], out var p) ? p : 587;
            var senderName = settings["SenderName"] ?? "eMotion";
            var senderEmail = settings["SenderEmail"];
            var username = settings["Username"] ?? senderEmail;
            var password = settings["Password"];

            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning(
                    "EmailSettings are not configured. Skipping e-mail to {Recipient}.", toEmail);
                return;
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
