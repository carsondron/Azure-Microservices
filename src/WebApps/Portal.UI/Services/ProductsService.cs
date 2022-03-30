using Portal.UI.Extensions;
using Portal.UI.Models;
using Portal.UI.Services.Interfaces;

namespace Portal.UI.Services
{
    public class ProductsService : IProductsService
    {
        private readonly HttpClient _client;

        public ProductsService(HttpClient client, ILogger<ProductsService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IEnumerable<ProductModel>> GetProducts()
        {
            var response = await _client.GetAsync("Products");
            return await response.ReadContentAs<List<ProductModel>>();
        }

        public async Task<ProductModel> GetProduct(string id)
        {
            var response = await _client.GetAsync($"Products/{id}");
            return await response.ReadContentAs<ProductModel>();
        }

        public async Task<IEnumerable<ProductModel>> GetProductsByCategory(string category)
        {
            var response = await _client.GetAsync($"Products/GetProductByCategory/{category}");
            return await response.ReadContentAs<List<ProductModel>>();
        }

        public async Task<ProductModel> CreateProduct(ProductModel model)
        {
            var response = await _client.PostAsJson($"Products", model);
            if (response.IsSuccessStatusCode)
                return await response.ReadContentAs<ProductModel>();
            else
            {
                throw new Exception("Something went wrong when calling api.");
            }
        }
    }
}