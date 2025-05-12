using System.Text.Json;
using Product_Manager_Mini_API.DTOs;
using Product_Manager_Mini_API.Models;

namespace Product_Manager_Mini_API.Services
{
    public class ProductService
    {

        private readonly IFileService _fileService;

        public ProductService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<IResult> GetAllProductsAsync()
        {
            try
            {
                var productsList = await _fileService.ReadProductsJsonAsync();
                return Results.Ok(productsList);
            }
            catch (JsonException ex)
            {
                return Results.Problem(
                    detail: $"Invalid JSON format: {ex.Message}",
                    title: "Bad Request",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (IOException ex)
            {
                return Results.Problem(
                    detail: $"File access error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Unexpected error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<IResult> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await FindProductByIdAsync(id);
                if (product == null)
                {
                    return Results.Problem(
                        detail: $"Product with ID {id} not found",
                        title: "Not Found",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.4",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }
                return Results.Ok(product);
            }
            catch (JsonException ex)
            {
                return Results.Problem(
                    detail: $"Invalid JSON format: {ex.Message}",
                    title: "Bad Request",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (IOException ex)
            {
                return Results.Problem(
                    detail: $"File access error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Unexpected error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        private async Task<Product?> FindProductByIdAsync(int id)
        {
            var productsList = await _fileService.ReadProductsJsonAsync();
            return productsList.FirstOrDefault(p => p.Id == id);
        }

        public async Task<IResult> CreateProductAsync(CreateProductDTO newProduct)
        {
            try
            {
                if (newProduct == null)
                {
                    return Results.Problem(
                        detail: "New product cannot be null.",
                        title: "Bad Request",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var invalidProperties = new List<string>();

                if (string.IsNullOrEmpty(newProduct.Name))
                    invalidProperties.Add("Name");
                if (newProduct.Price <= 0)
                    invalidProperties.Add("Price");
                if (string.IsNullOrEmpty(newProduct.Category))
                    invalidProperties.Add("Category");

                if (invalidProperties.Any())
                {
                    return Results.Problem(
                        detail: $"Invalid product data: Missing or invalid properties: {string.Join(", ", invalidProperties)}",
                        title: "Bad Request",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var productsList = await _fileService.ReadProductsJsonAsync();

                int newId = productsList.Any() ? productsList.Max(p => p.Id) + 1 : 1;

                var product = new Product
                {
                    Id = newId,
                    Name = newProduct.Name,
                    Price = newProduct.Price,
                    Category = newProduct.Category
                };

                productsList.Add(product);

                await _fileService.WriteProductsListToJsonAsync(productsList);

                return Results.Created($"/products/{product.Id}", product);
            }
            catch (JsonException ex)
            {
                return Results.Problem(
                    detail: $"Invalid JSON format: {ex.Message}",
                    title: "Bad Request",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (IOException ex)
            {
                return Results.Problem(
                    detail: $"File access error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Unexpected error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<IResult> UpdateProductAsync(int id, UpdateProductDTO updateProductDTO)
        {
            try
            {
                if (updateProductDTO == null)
                {
                    return Results.Problem(
                        detail: "Updated product cannot be null.",
                        title: "Bad Request",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var product = await FindProductByIdAsync(id);
                if (product == null)
                {
                    return Results.Problem(
                        detail: $"Product with ID {id} not found",
                        title: "Not Found",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.4",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }

                var productsList = await _fileService.ReadProductsJsonAsync();

                if (!string.IsNullOrEmpty(updateProductDTO.Name))
                    product.Name = updateProductDTO.Name;
                if (updateProductDTO.Price.HasValue)
                    product.Price = updateProductDTO.Price.Value;
                if (!string.IsNullOrEmpty(updateProductDTO.Category))
                    product.Category = updateProductDTO.Category;

                await _fileService.WriteProductsListToJsonAsync(productsList);

                return Results.Ok(product);
            }
            catch (JsonException ex)
            {
                return Results.Problem(
                    detail: $"Invalid JSON format: {ex.Message}",
                    title: "Bad Request",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (IOException ex)
            {
                return Results.Problem(
                    detail: $"File access error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Unexpected error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        public async Task<IResult> DeleteProductAsync(int id)
        {
            try
            {
                var product = await FindProductByIdAsync(id);
                if (product == null)
                {
                    return Results.Problem(
                        detail: $"Product with ID {id} not found",
                        title: "Not Found",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.4",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }

                var productsList = await _fileService.ReadProductsJsonAsync();
                productsList.Remove(product);

                await _fileService.WriteProductsListToJsonAsync(productsList);

                return Results.NoContent();
            }
            catch (JsonException ex)
            {
                return Results.Problem(
                    detail: $"Invalid JSON format: {ex.Message}",
                    title: "Bad Request",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
            catch (IOException ex)
            {
                return Results.Problem(
                    detail: $"File access error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Unexpected error: {ex.Message}",
                    title: "Internal Server Error",
                    type: "https://datatracker.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }
    }
}
