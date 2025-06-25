using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string templateId, Dictionary<string, string> dynamicData);
    }
} 