using Products.API.Entities;
using MongoDB.Driver;

namespace Products.API.Data.Interfaces
{
    public interface IProductContext
    {
        IMongoCollection<ProductItem> Products { get; }
    }
}