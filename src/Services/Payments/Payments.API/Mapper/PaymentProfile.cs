using AutoMapper;
using EventBusMessages.Events;
using Payments.API.PaymentProcessor;

namespace Payments.API.Mapper
{
	public class PaymentProfile : Profile
	{
		public PaymentProfile()
		{
			CreateMap<PaymentProcessorCommand, PaymentExecutionEvent>().ReverseMap();
		}
	}
}