using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Email;
using AjoCoreBackend.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public SmtpEmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // In a production app, we would log this error.
                // For the hackathon, we'll swallow it or write to console so it doesn't crash the request if credentials aren't set up.
                Console.WriteLine($"Failed to send email to {to}: {ex.Message}");
            }
        }
    }
}
