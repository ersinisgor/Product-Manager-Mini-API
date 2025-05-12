
using System;
using System.Formats.Asn1;
using System.IO;
using System.Text.Json;
using Product_Manager_Mini_API.DTOs;
using Product_Manager_Mini_API.Models;
using Product_Manager_Mini_API.Services;

namespace Product_Manager_Mini_API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register services for Dependency Injection
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<ProductService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Data Source");
            string filePath = Path.Combine(directoryPath, "products.json");

            app.MapGet("/products", async (ProductService productService) => await productService.GetAllProductsAsync());

            app.MapGet("/products/{id}", async (ProductService productService, int id) => await productService.GetProductByIdAsync(id));

            app.MapPost("/products", async (ProductService productService, CreateProductDTO newProduct) => await productService.CreateProductAsync(newProduct));

            app.MapPut("/products/{id}", async (int id, UpdateProductDTO updateProduct, ProductService productService) => await productService.UpdateProductAsync(id, updateProduct));

            app.MapDelete("/products/{id}", async (int id) =>
            {
                try
                {


                    if (!File.Exists(filePath))
                    {
                        return Results.Problem(
                            detail: $"File {filePath} does not exist",
                            statusCode: StatusCodes.Status404NotFound
                        );
                    }

                    var productList = new List<Product>();

                    using (var streamReader = new StreamReader(filePath))
                    {
                        string json = await streamReader.ReadToEndAsync();
                        if (!string.IsNullOrWhiteSpace(json) && json.Trim() != "[]")
                        {
                            productList = JsonSerializer.Deserialize<List<Product>>(json, jsonOptions) ?? new List<Product>();
                        }
                    }

                    var product = productList.FirstOrDefault(p => p.Id == id);
                    if (product == null)
                    {
                        return Results.Problem(
                            detail: $"Product with ID {id} not found",
                            statusCode: StatusCodes.Status404NotFound
                        );
                    }

                    productList.Remove(product);

                    string productListJson = JsonSerializer.Serialize(productList, jsonOptions);

                    using (var streamWriter = new StreamWriter(filePath))
                    {
                        await streamWriter.WriteAsync(productListJson);
                    }

                    return Results.NoContent();
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
            });

            app.Run();
        }
    }
}
