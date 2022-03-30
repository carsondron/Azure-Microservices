using Portal.UI.Extensions;
using Portal.UI.Models;
using Portal.UI.Services.Interfaces;

namespace Portal.UI.Services
{
    public class CartService : ICartService
    {
        private readonly HttpClient _client;

        public CartService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<CartModel> GetCartByUsername(string username)
        {
            var response = await _client.GetAsync($"Cart/{username}");
            return await response.ReadContentAs<CartModel>();
        }

        public async Task<CartModel> UpdateCart(CartModel model)
        {
            var response = await _client.PostAsJson($"Cart", model);
            if (response.IsSuccessStatusCode)
                return await response.ReadContentAs<CartModel>();
            else
            {
                throw new Exception("Something went wrong when calling api.");
            }
        }

        public async Task CheckoutCart(CartCheckoutModel model)
        {
            var response = await _client.PostAsJson($"Cart/Checkout", model);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Something went wrong when calling api.");
            }
        }
    }
}