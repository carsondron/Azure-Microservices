using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Portal.UI.Views
{
    [Authorize]
    public class OrderConfirmationModel : PageModel
    {
        public string Message { get; set; }

        public void OnGetOrderSubmitted()
        {
            Message = "Your order submitted successfully.";
        }
    }
}
