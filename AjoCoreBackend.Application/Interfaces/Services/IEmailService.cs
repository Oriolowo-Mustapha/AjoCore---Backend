using System.Threading.Tasks;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}
