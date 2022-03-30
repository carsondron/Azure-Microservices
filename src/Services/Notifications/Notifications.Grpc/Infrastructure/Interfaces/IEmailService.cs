using Notifications.Grpc.Models;

namespace Notifications.Grpc.Infrastructure.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmail(Email email);
    }
}