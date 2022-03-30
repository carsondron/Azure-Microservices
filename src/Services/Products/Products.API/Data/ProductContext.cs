using MongoDB.Driver;
using Products.API.Data.Interfaces;
using Products.API.Entities;

namespace Products.API.Data
{
    public class ProductContext : IProductContext
    {
        public ProductContext(IConfiguration configuration, ILogger<ProductContext> logger)
        {            
            var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

            Products = database.GetCollection<ProductItem>(configuration.GetValue<string>("DatabaseSettings:CollectionName"));
            ProductContextSeed.SeedData(Products);
        }

        public IMongoCollection<ProductItem> Products { get; }
    }
}