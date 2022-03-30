using Portal.UI.Models;

namespace Portal.UI.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartModel> GetCartByUsername(string username);
        Task<CartModel> UpdateCart(CartModel model);
        Task CheckoutCart(CartCheckoutModel model);
    }
}