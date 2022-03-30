using Portal.UI.Models;

namespace Portal.UI.Services.Interfaces
{
    public interface IProductsService
    {
        Task<IEnumerable<ProductModel>> GetProducts();
        Task<IEnumerable<ProductModel>> GetProductsByCategory(string category);
        Task<ProductModel> GetProduct(string id);
        Task<ProductModel> CreateProduct(ProductModel model);
    }
}