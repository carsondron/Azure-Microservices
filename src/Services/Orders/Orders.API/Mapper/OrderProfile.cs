using AutoMapper;
using EventBusMessages.Events;
using Orders.Application.Features.Orders.Commands.CheckoutOrder;

namespace Orders.API.Mapper
{
	public class OrderingProfile : Profile
	{
		public OrderingProfile()
		{
			CreateMap<CheckoutOrderCommand, CartCheckoutEvent>().ReverseMap();
		}
	}
}