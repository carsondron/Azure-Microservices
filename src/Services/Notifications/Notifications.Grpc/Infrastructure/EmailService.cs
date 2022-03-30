using Notifications.Grpc.Infrastructure.Interfaces;
using Notifications.Grpc.Models;

namespace Notifications.Grpc.Infrastructure
{
    public class EmailService : IEmailService
    {
        public ILogger<EmailService> _logger { get; }

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmail(Email email)
        {
            _logger.LogInformation($"Simulating sending email to {email.Recipient}");

            return true;
        }
    }
}