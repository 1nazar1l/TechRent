using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace TechRent.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _configuration["EmailSettings:Username"],
                _configuration["EmailSettings:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}