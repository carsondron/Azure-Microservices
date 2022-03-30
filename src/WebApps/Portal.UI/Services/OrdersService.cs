using Portal.UI.Extensions;
using Portal.UI.Models;
using Portal.UI.Services.Interfaces;

namespace Portal.UI.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly HttpClient _client;

        public OrdersService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IEnumerable<OrderResponseModel>> GetOrdersByUsername(string username)
        {
            var response = await _client.GetAsync($"Orders/{username}");
            return await response.ReadContentAs<List<OrderResponseModel>>();
        }
    }
}