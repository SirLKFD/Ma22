using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _senderEmail;
        public EmailService(IConfiguration config)
        {
            _apiKey = config["SendGrid:ApiKey"];
            _senderEmail = config["SendGrid:SenderEmail"];
        }

        public async Task SendEmailAsync(string toEmail, string templateId, Dictionary<string, string> dynamicData)
        {
            var client = new SendGridClient(_apiKey);
            var msg = new SendGridMessage();
            msg.SetFrom(_senderEmail);
            msg.AddTo(toEmail);
            msg.SetTemplateId(templateId);
            msg.SetTemplateData(dynamicData);
            await client.SendEmailAsync(msg);
        }
    }
} 