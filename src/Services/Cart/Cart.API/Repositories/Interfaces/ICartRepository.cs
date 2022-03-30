using Cart.API.Entities;

namespace Cart.API.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<ShoppingCart> GetCart(string username);
        Task<ShoppingCart> UpdateCart(ShoppingCart cart);
        Task DeleteCart(string username);
    }
}