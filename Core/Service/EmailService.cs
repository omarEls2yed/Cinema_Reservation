using ServiceAbstraction;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Service
{
    public class EmailService(IConfiguration config) : IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var settings = config.GetSection("EmailSettings");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(settings["SenderName"] ?? "System", settings["SenderEmail"]));
            emailMessage.To.Add(MailboxAddress.Parse(email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new SmtpClient();
            try
            {
                int port = int.TryParse(settings["Port"], out var p) ? p : 587;
                await client.ConnectAsync(settings["Host"], port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(settings["UserName"], settings["Password"]);
                await client.SendAsync(emailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception("Email failed to send. Check your SMTP settings.", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
