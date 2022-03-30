using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portal.UI.Models;
using Portal.UI.Services.Interfaces;

namespace Portal.UI.Views
{
    [Authorize]
    public class IndexViewModel : PageModel
    {
        private readonly IProductsService _productsService;
        private readonly ICartService _cartService;

        public IndexViewModel(IProductsService productsService, ICartService cartService)
        {
            _productsService = productsService ?? throw new ArgumentNullException(nameof(productsService));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        public IEnumerable<ProductModel> ProductList { get; set; } = new List<ProductModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            ProductList = await _productsService.GetProducts();
            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(string productId)
        {
            var product = await _productsService.GetProduct(productId);

            var username = "swn";
            var cart = await _cartService.GetCartByUsername(username);

            int index = cart.Items.FindIndex(p => p.ProductId == productId);
            if (index == -1)
            {
                cart.Items.Add(new CartItemModel
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1
                });
            } else
            {
                cart.Items[index].Quantity++;
            }

            var cartUpdated = await _cartService.UpdateCart(cart);
            return RedirectToPage("Cart");
        }
    }
}
