using Microsoft.EntityFrameworkCore;
using Orders.Application.Contracts.Persistence;
using Orders.Domain.Entities;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Repositories
{
    public class OrderRepository : RepositoryBase<OrderItem>, IOrderRepository
    {
        public OrderRepository(OrderContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<OrderItem>> GetOrdersByUsername(string username)
        {
            var orderList = await _dbContext.Orders
                                .Where(o => o.Username == username)
                                .ToListAsync();
            return orderList;
        }
    }
}