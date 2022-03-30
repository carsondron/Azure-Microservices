using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portal.UI.Models;
using Portal.UI.Services.Interfaces;

namespace Portal.UI
{
    [Authorize]
    public class CartViewModel : PageModel
    {
        private readonly ICartService _cartService;

        public CartViewModel(ICartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        public CartModel Cart { get; set; } = new CartModel();

        public async Task<IActionResult> OnGetAsync()
        {
            var username = "swn";
            Cart = await _cartService.GetCartByUsername(username);

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveToCartAsync(string productId)
        {
            var username = "swn";
            var cart = await _cartService.GetCartByUsername(username);

            var item = cart.Items.Single(x => x.ProductId == productId);
            cart.Items.Remove(item);

            var cartUpdated = await _cartService.UpdateCart(cart);

            return RedirectToPage();
        }
    }
}