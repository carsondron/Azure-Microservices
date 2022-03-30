using AutoMapper;
using Cart.API.Entities;
using EventBusMessages.Events;

namespace Cart.API.Mapper
{
	public class CartProfile : Profile
	{
		public CartProfile()
		{
			CreateMap<CartCheckout, CartCheckoutEvent>().ReverseMap();
		}
	}
}