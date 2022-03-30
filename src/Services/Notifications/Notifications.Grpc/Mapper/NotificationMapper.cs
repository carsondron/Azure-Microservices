using AutoMapper;
using Notifications.Grpc.Models;
using Notifications.Grpc.Protos;

namespace Notifications.Grpc.Mapper
{
	public class NotificationMapper : Profile
	{
		public NotificationMapper()
		{
			CreateMap<EmailMessageModel, Email>().ReverseMap();
		}
	}
}