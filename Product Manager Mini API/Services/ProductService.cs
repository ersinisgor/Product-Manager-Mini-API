using System.Text.Json;
using Product_Manager_Mini_API.Models;

namespace Product_Manager_Mini_API.Services
{
    public class ProductService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _directoryPath;
        private readonly string _filePath;

        public ProductService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            _directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Data Source");
            _filePath = Path.Combine(_directoryPath, "products.json");
        }


        public async Task<List<Product>> GetAllProducts()
        {
            return new List<Product>();
        }
    }
}
