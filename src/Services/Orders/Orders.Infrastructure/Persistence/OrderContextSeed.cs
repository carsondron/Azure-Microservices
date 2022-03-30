using Microsoft.Extensions.Logging;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            if (!orderContext.Orders.Any())
            {
                orderContext.Orders.AddRange(GetPreconfiguredOrders());
                await orderContext.SaveChangesAsync();
                logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
            }
        }

        private static IEnumerable<OrderItem> GetPreconfiguredOrders()
        {
            return new List<OrderItem>
            {
                new OrderItem() {Username = "joe.bloggs", FirstName = "Joe", LastName = "Bloggs", EmailAddress = "joe.bloggs@example.com", AddressLine = "Mars Street", Country = "Mars", TotalPrice = 50 }
            };
        }
    }
}