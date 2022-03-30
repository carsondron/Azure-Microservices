using Cart.API.Entities;
using Cart.API.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Cart.API.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDistributedCache _redisCache;

        public CartRepository(IDistributedCache cache)
        {
            _redisCache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<ShoppingCart> GetCart(string username)
        {
            var cart = await _redisCache.GetStringAsync(username);

            if (String.IsNullOrEmpty(cart))
                return null;

            return JsonConvert.DeserializeObject<ShoppingCart>(cart);
        }

        public async Task<ShoppingCart> UpdateCart(ShoppingCart cart)
        {
            await _redisCache.SetStringAsync(cart.Username, JsonConvert.SerializeObject(cart));

            return await GetCart(cart.Username);
        }

        public async Task DeleteCart(string username)
        {
            await _redisCache.RemoveAsync(username);
        }
    }
}