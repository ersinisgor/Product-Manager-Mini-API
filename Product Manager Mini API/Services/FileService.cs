using System.Text.Json;
using Product_Manager_Mini_API.Models;

namespace Product_Manager_Mini_API.Services
{
    public interface IFileService
    {
        Task<List<Product>> ReadProductsJsonAsync();
    }

    public class FileService: IFileService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _directoryPath;
        private readonly string _filePath;

        public FileService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            _directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Data Source");
            _filePath = Path.Combine(_directoryPath, "products.json");
        }

        public async Task<List<Product>> ReadProductsJsonAsync()
        {
            var productsList = new List<Product>();

            if (!File.Exists(_filePath))
            {
                return productsList;
            }

            using var streamReader = new StreamReader(_filePath);
            var json = await streamReader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
            {
                return productsList;
            }

            productsList = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
            return productsList;
        }
    }
}
