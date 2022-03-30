using Portal.UI.Models;

namespace Portal.UI.Services.Interfaces
{
    public interface IOrdersService
    {
        Task<IEnumerable<OrderResponseModel>> GetOrdersByUsername(string username);
    }
}