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

        /// <summary>
        /// Finds a product by its ID and returns the product along with the full product list.
        /// </summary>
        /// <param name="id">The ID of the product to find.</param>
        /// <returns>A tuple containing the product (if found, otherwise null) and the full product list.</returns>
        /// <exception cref="JsonException">If the JSON format is invalid.</exception>
        /// <exception cref="IOException">If there is a file access error.</exception>
        private async Task<Product?> FindProductByIdAsync(int id)
        {
            var productsList = await _fileService.ReadProductsJsonAsync();
            return productsList.FirstOrDefault(p => p.Id == id);
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

        /// <summary>
        /// Deletes a product from the products.json file.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>An IResult indicating success or an error response.</returns>
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
