using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Notifications.Grpc.Infrastructure.Interfaces;
using Notifications.Grpc.Models;
using Notifications.Grpc.Protos;
using static Notifications.Grpc.Protos.SendNotificationRequest.Types;

namespace Notifications.Grpc.Services
{
	public class NotificationService : NotificationsProtoService.NotificationsProtoServiceBase
	{
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IEmailService emailService, IMapper mapper, ILogger<NotificationService> logger)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<Empty> SendNotification(SendNotificationRequest request, ServerCallContext context)
        {
            if (request.Type == NotificationType.Email)
            {
                var emailDetails = _mapper.Map<Email>(request.EmailMessage);
                try
                {
                    await _emailService.SendEmail(emailDetails);
                    return await Task.FromResult(new Empty());
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unable to send notification to {emailDetails.Recipient} due to an error with the mail service: {ex.Message}");
                    throw new RpcException(new Status(StatusCode.Internal, $"Unable to send notification to {emailDetails.Recipient}", ex));
                }
            } else
            {
                throw new RpcException(new Status(StatusCode.Internal, $"Unknown notification type={request.Type} specified."));
            }
        }
    }
}