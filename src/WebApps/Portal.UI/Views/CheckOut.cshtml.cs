using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Portal.UI.Models;
using Portal.UI.Services.Interfaces;

namespace Portal.UI.Views
{
    [Authorize]
    public class CheckOutModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrdersService _ordersService;
        private readonly ILogger _logger;

        public CheckOutModel(ICartService cartService, IOrdersService ordersService, ILogger<CheckOutModel> logger)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _ordersService = ordersService ?? throw new ArgumentNullException(nameof(ordersService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [BindProperty]
        public CartCheckoutModel Order { get; set; }

        public CartModel Cart { get; set; } = new CartModel();

        public async Task<IActionResult> OnGetAsync()
        {
            var userName = "swn";
            Cart = await _cartService.GetCartByUsername(userName);

            return Page();
        }

        public async Task<IActionResult> OnPostCheckOutAsync()
        {
            var userName = "swn";
            Cart = await _cartService.GetCartByUsername(userName);

            Order.Username = userName;
            Order.TotalPrice = Cart.TotalPrice;

            await _cartService.CheckoutCart(Order);

            return RedirectToPage("OrderConfirmation", "OrderSubmitted");
        }
    }
}