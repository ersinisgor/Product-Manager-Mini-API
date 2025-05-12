using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Gets all products from the products.json file.
        /// </summary>
        /// <returns>An IResult containing the list of products or an error response.</returns>
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

        /// <summary>
        /// Gets a product by its ID from the products.json file.
        /// </summary>
        /// <param name="id">The ID of the product to retrieve.</param>
        /// <returns>An IResult containing the product or an error response.</returns>
        public async Task<IResult> GetProductByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Results.Problem(
                        detail: "Product ID must be greater than zero.",
                        title: "Bad Request",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var (product, _) = await RetrieveProductWithProductsListAsync(id);
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

        /// <summary>
        /// Finds a product by its ID and returns the product along with the full product list.
        /// </summary>
        /// <param name="id">The ID of the product to find.</param>
        /// <returns>A tuple containing the product (if found, otherwise null) and the full product list.</returns>
        /// <exception cref="JsonException">If the JSON format is invalid.</exception>
        /// <exception cref="IOException">If there is a file access error.</exception>
        private async Task<(Product?, List<Product>)> RetrieveProductWithProductsListAsync(int id)
        {
            var productsList = await _fileService.ReadProductsJsonAsync();
            var product = productsList.FirstOrDefault(p => p.Id == id);
            return (product, productsList);
        }

        /// <summary>
        /// Creates a new product and adds it to the products.json file.
        /// </summary>
        /// <param name="newProduct">The product data to create.</param>
        /// <returns>An IResult containing the created product or an error response.</returns>
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

                if (string.IsNullOrWhiteSpace(newProduct.Name))
                    invalidProperties.Add("Name");
                if (newProduct.Name?.Length > 100)
                    invalidProperties.Add("Name (exceeds 100 characters)");
                if (newProduct.Price <= 0)
                    invalidProperties.Add("Price");
                if (string.IsNullOrWhiteSpace(newProduct.Category))
                    invalidProperties.Add("Category");
                if (newProduct.Category?.Length > 50)
                    invalidProperties.Add("Category (exceeds 50 characters)");

                if (invalidProperties.Any())
                {
                    return Results.Problem(
                        detail: $"Invalid product data: {string.Join(", ", invalidProperties)}",
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

        /// <summary>
        /// Updates an existing product in the products.json file.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="updateProductDTO">The updated product data.</param>
        /// <returns>An IResult containing the updated product or an error response.</returns>
        public async Task<IResult> UpdateProductAsync(int id, UpdateProductDTO updateProductDTO)
        {
            try
            {
                if (id <= 0)
                {
                    return Results.Problem(
                        detail: "Product ID must be greater than zero.",
                        title: "Bad Request",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                if (updateProductDTO == null)
                {
                    return Results.Problem(
                        detail: "Updated product cannot be null.",
                        title: "Bad Request",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var (product, productsList) = await RetrieveProductWithProductsListAsync(id);
                if (product == null)
                {
                    return Results.Problem(
                        detail: $"Product with ID {id} not found",
                        title: "Not Found",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.4",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }

                var invalidProperties = new List<string>();

                if (updateProductDTO.Name != null)
                {
                    if (string.IsNullOrWhiteSpace(updateProductDTO.Name))
                        invalidProperties.Add("Name (empty or whitespace)");
                    if (updateProductDTO.Name.Length > 100)
                        invalidProperties.Add("Name (exceeds 100 characters)");
                }
                if (updateProductDTO.Price.HasValue && updateProductDTO.Price <= 0)
                    invalidProperties.Add("Price (must be greater than zero)");
                if (updateProductDTO.Category != null)
                {
                    if (string.IsNullOrWhiteSpace(updateProductDTO.Category))
                        invalidProperties.Add("Category (empty or whitespace)");
                    if (updateProductDTO.Category.Length > 50)
                        invalidProperties.Add("Category (exceeds 50 characters)");
                }

                if (invalidProperties.Any())
                {
                    return Results.Problem(
                        detail: $"Invalid product data: {string.Join(", ", invalidProperties)}",
                        title: "Bad Request",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                bool hasChanges = false;
                if (!string.IsNullOrEmpty(updateProductDTO.Name))
                {
                    product.Name = updateProductDTO.Name;
                    hasChanges = true;
                }
                if (updateProductDTO.Price.HasValue)
                {
                    product.Price = updateProductDTO.Price.Value;
                    hasChanges = true;
                }
                if (!string.IsNullOrEmpty(updateProductDTO.Category))
                {
                    product.Category = updateProductDTO.Category;
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    return Results.Ok(product);
                }

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

        /// <summary>
        /// Deletes a product from the products.json file.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>An IResult indicating success or an error response.</returns>
        public async Task<IResult> DeleteProductAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Results.Problem(
                        detail: "Product ID must be greater than zero.",
                        title: "Bad Request",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.1",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var (product, productsList) = await RetrieveProductWithProductsListAsync(id);
                if (product == null)
                {
                    return Results.Problem(
                        detail: $"Product with ID {id} not found",
                        title: "Not Found",
                        type: "https://datatracker.ietf.org/html/rfc7231#section-6.5.4",
                        statusCode: StatusCodes.Status404NotFound
                    );
                }

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
