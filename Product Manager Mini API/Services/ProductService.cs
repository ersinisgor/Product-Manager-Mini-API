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

        private async Task<List<Product>> ReadProductsJsonAsync()
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


        public async Task<IResult> GetAllProductsAsync()
        {
            try
            {
                var productsList = await ReadProductsJsonAsync();
                return Results.Ok(productsList);
            }
            catch (JsonException ex)
            {
                return Results.Problem(
                    detail: $"Invalid JSON format: {ex.Message}",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (IOException ex)
            {
                return Results.Problem(
                    detail: $"File access error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Unexpected error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }

        }

        public async Task<IResult> GetProductByIdAsync(int id)
        {
            try
            {
                var productsList = await ReadProductsJsonAsync();

                var product = productsList.FirstOrDefault(p => p.Id == id);
                if (product == null)
                {
                    return Results.Problem(
                        detail: $"Product with ID {id} not found",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }

                return Results.Ok(product);
            }
            catch (JsonException ex)
            {
                return Results.Problem(
                    detail: $"Invalid JSON format: {ex.Message}",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (IOException ex)
            {
                return Results.Problem(
                    detail: $"File access error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Unexpected error: {ex.Message}",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }
    }
}
