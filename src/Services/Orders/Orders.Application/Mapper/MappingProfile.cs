using AutoMapper;
using EventBusMessages.Events;
using Orders.Application.Features.Orders.Commands.CheckoutOrder;
using Orders.Application.Features.Orders.Commands.UpdateOrder;
using Orders.Application.Features.Orders.Queries.GetOrdersList;
using Orders.Domain.Entities;

namespace Orders.Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OrderItem, OrderVm>().ReverseMap();
            CreateMap<OrderItem, CheckoutOrderCommand>().ReverseMap();
            CreateMap<OrderItem, UpdateOrderCommand>().ReverseMap();
            CreateMap<PaymentExecutionEvent, CheckoutOrderCommand>().ReverseMap();
        }
    }
}

