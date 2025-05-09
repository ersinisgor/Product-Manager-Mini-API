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


        public async Task<IResult> GetAllProductsAsync()
        {
            var productList = new List<Product>();
            try
            {
                //filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data Source", "products.json");
                if (!File.Exists(_filePath))
                {
                    return Results.Problem(
                    detail: $"File {_filePath} does not exist",
                    statusCode: StatusCodes.Status404NotFound
                    );

                }

                using var streamReader = new StreamReader(_filePath);
                string json = await streamReader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
                {
                    return Results.Ok(productList);
                }

                productList = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
                if (productList.Count <= 0)
                {
                    return Results.Problem(
                    detail: "Invalid JSON format",
                    statusCode: StatusCodes.Status400BadRequest
                    );
                }

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

            return Results.Ok(productList);
        }
    }
}
