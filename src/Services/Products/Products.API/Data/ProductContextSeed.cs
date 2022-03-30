using Products.API.Entities;
using MongoDB.Driver;
using Serilog;

namespace Products.API.Data
{
    public class ProductContextSeed
    {
        public static void SeedData(IMongoCollection<ProductItem> productCollection)
        {
            bool existProduct = productCollection.Find(p => true).Any();
            if (!existProduct)
            {
                productCollection.InsertManyAsync(GetPreconfiguredProducts());
                Log.Information("Seed products database with {count} elements", GetPreconfiguredProducts().ToList().Count);
            }
        }

        private static IEnumerable<ProductItem> GetPreconfiguredProducts()
        {
            return new List<ProductItem>()
            {
                new ProductItem()
                {
                    Id = "621589e812f55e4bd48c6f4f",
                    Name = "Acer acreanon laptop with 16GB RAM/500GB HDD",
                    Summary = "Powerful enough to use as a mining rig.",
                    Description = "12th Gen Intel Core i7-12230U processor, 15.5 Inch Display, 16GB RAM with GTX1080 8GB Graphics, Warranty: 12 Months Warranty",
                    Price = 3450.00M,
                    Category = "Laptop"
                },
                new ProductItem()
                {
                    Id = "62158a3ff97cfed24b1e949a",
                    Name = "HP Celeron laptop with 8GB RAM/250GB HDD",
                    Summary = "Perfect as a business laptop.",
                    Description = "9th Gen Intel Core i7-9120U processor, 15.5 Inch Display, 8GB RAM with Radeon 560x 2GB Graphics, Warranty: 12 Months Warranty",
                    Price = 1440.00M,
                    Category = "Laptop"
                },
                new ProductItem()
                {
                    Id = "62158a444c12ea74a26f7d89",
                    Name = "Acer Modifier laptop with 8GB RAM/500GB HDD",
                    Summary = "Perform for work and general browsing.",
                    Description = "9th Gen Intel Core i5-9350U processor, 15.5 Inch Display, 8GB RAM with Radeon 530 2GB Graphics, Warranty: 12 Months Warranty",
                    Price = 1650.00M,
                    Category = "Laptop"
                },
                new ProductItem()
                {
                    Id = "62158a5511d28e054d9c894b",
                    Name = "ASUS Modifier Gaming laptop with 12GB RAM/500GB HDD",
                    Summary = "Perfect for gaming and for schoool students.",
                    Description = "8th Gen Intel Core i5-8250U processor, 15.5 Inch | Antiglare Display, 12GB RAM with Radeon 530 2GB Graphics, EMI Starts at 1726. No cost EMI available, Warranty: 6 Months Warranty",
                    Price = 2370.00M,
                    Category = "Laptop"
                }
            };
        }
    }
}