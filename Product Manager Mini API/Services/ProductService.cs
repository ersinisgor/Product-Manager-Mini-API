using System.Text.Json;
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
                var productsList = await _fileService.ReadProductsJsonAsync();

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
