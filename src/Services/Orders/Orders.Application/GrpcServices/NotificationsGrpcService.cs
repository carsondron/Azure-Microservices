using Google.Protobuf.WellKnownTypes;
using Notifications.Grpc.Protos;
using Orders.Application.GrpcServices.Interfaces;

namespace Orders.Application.GrpcServices
{
    public class NotificationsGrpcService : INotificationsGrpcService
    {
        private readonly NotificationsProtoService.NotificationsProtoServiceClient _notificationsProtoService;

        public NotificationsGrpcService(NotificationsProtoService.NotificationsProtoServiceClient notificationsProtoService)
        {
            _notificationsProtoService = notificationsProtoService ?? throw new ArgumentNullException(nameof(notificationsProtoService));
        }

        public async Task<Empty> SendEmail(string recipient, string emailSubject, string emailBody)
        {
            var sendEmailRequest = new SendNotificationRequest
            {
                Type = SendNotificationRequest.Types.NotificationType.Email,
                EmailMessage = new EmailMessageModel { Recipient = recipient, Subject = emailSubject, Body = emailBody }
            };

            return await Task.FromResult(_notificationsProtoService.SendNotification(sendEmailRequest));
        }
    }
}