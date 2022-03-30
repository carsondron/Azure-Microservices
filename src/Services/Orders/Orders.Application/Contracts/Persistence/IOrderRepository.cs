using Orders.Domain.Entities;

namespace Orders.Application.Contracts.Persistence
{
    public interface IOrderRepository : IAsyncRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetOrdersByUsername(string username);
    }
}