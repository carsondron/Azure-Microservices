using Products.API.Entities;

namespace Products.API.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductItem>> GetProducts();
        Task<ProductItem> GetProduct(string id);
        Task<IEnumerable<ProductItem>> GetProductByName(string name);
        Task<IEnumerable<ProductItem>> GetProductByCategory(string categoryName);

        Task CreateProduct(ProductItem product);
        Task<bool> UpdateProduct(ProductItem product);
        Task<bool> DeleteProduct(string id);
    }
}