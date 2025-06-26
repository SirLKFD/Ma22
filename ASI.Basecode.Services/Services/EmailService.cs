using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace ASI.Basecode.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly ILogger _logger;
        public EmailService(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _apiKey = config["SendGrid:ApiKey"];
            _senderEmail = config["SendGrid:SenderEmail"];
            _logger = loggerFactory.CreateLogger<EmailService>();
        }

        public async Task SendEmailAsync(string toEmail, string templateId, Dictionary<string, string> dynamicData)
        {
            _logger.LogInformation("Attempting to send email to {ToEmail} with template {TemplateId}", toEmail, templateId);
            try
            {
                var client = new SendGridClient(_apiKey);
                var msg = new SendGridMessage();
                msg.SetFrom(_senderEmail);
                msg.AddTo(toEmail);
                msg.SetTemplateId(templateId);
                msg.SetTemplateData(dynamicData);
                var response = await client.SendEmailAsync(msg);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                }
                else
                {
                    _logger.LogError("Failed to send email to {ToEmail}. Status: {StatusCode}", toEmail, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email to {ToEmail}", toEmail);
                throw;
            }
        }
    }
} 