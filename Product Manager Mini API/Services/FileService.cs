using System.Text.Json;
using Product_Manager_Mini_API.Models;

namespace Product_Manager_Mini_API.Services
{
    public interface IFileService
    {
        Task<List<Product>> ReadProductsJsonAsync();
        Task WriteProductsListToJsonAsync(List<Product> productsList);
    }

    public class FileService : IFileService
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

        /// <summary>
        /// Reads products from the products.json file and returns them as a list.
        /// </summary>
        /// <returns>A list of products.</returns>
        /// <exception cref="JsonException">If the JSON format is invalid.</exception>
        /// <exception cref="IOException">If there is a file access error.</exception>
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

        /// <summary>
        /// Ensures the directory for the products.json file exists and creates an empty file if it doesn't.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task EnsureDirectoryExists()
        {
            Directory.CreateDirectory(_directoryPath);
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Writes the list of products to the products.json file.
        /// </summary>
        /// <param name="productsList">The list of products to write.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="IOException">If there is a file access error.</exception>
        public async Task WriteProductsListToJsonAsync(List<Product> productsList)
        {
            await EnsureDirectoryExists();
            string productListJson = JsonSerializer.Serialize(productsList, _jsonOptions);
            using var streamWriter = new StreamWriter(_filePath);
            await streamWriter.WriteAsync(productListJson);
        }
    }
}
