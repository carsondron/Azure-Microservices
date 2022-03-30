using Google.Protobuf.WellKnownTypes;

namespace Orders.Application.GrpcServices.Interfaces
{
	public interface INotificationsGrpcService
	{
		Task<Empty> SendEmail(string recipient, string emailSubject, string emailBody);
	}
}