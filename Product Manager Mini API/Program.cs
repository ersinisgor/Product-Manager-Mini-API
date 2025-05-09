
using System;
using System.Formats.Asn1;
using System.IO;
using System.Text.Json;
using Product_Manager_Mini_API.DTOs;
using Product_Manager_Mini_API.Models;

namespace Product_Manager_Mini_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //string file = Directory.GetCurrentDirectory();
            //string filePath = Path.Combine(file, "Data", "products.json");
            //Console.WriteLine(file);

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            app.MapGet("/products", async () =>
            {
                var productList = new List<Product>();
                try
                {
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data Source", "products.json");
                    if (!File.Exists(filePath))
                    {
                        return Results.Problem(
                        detail: $"File {filePath} does not exist",
                        statusCode: StatusCodes.Status404NotFound
                        );

                    }

                    using var streamReader = new StreamReader(filePath);
                    string json = await streamReader.ReadToEndAsync();
                    if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
                    {
                        return Results.Ok(productList);
                    }

                    productList = JsonSerializer.Deserialize<List<Product>>(json, jsonOptions) ?? new List<Product>();
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
            });

            app.MapGet("/products/{id}", async (int id) =>
            {
                var productList = new List<Product>();
                try
                {
                    if (!File.Exists(filePath))
                    {
                        return Results.Problem(
                            detail: $"File {filePath} does not exist",
                            statusCode: StatusCodes.Status404NotFound
                        );
                    }

                    using var streamReader = new StreamReader(filePath);
                    string json = await streamReader.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(json) && json.Trim() != "[]")
                    {
                        productList = JsonSerializer.Deserialize<List<Product>>(json, jsonOptions) ?? new List<Product>();
                    }

                    var product = productList.FirstOrDefault(p => p.Id == id);
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
            });

            app.MapPost("/products", async (Product newProduct) =>
            {
                try
                {
                    if (newProduct == null)
                    {
                        return Results.Problem(
                            detail: "New product cannot be null.",
                            statusCode: StatusCodes.Status400BadRequest
                        );
                    }

                    var invalidProperties = new List<string>();

                    if (newProduct.Id <= 0)
                        invalidProperties.Add("Id");
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
                            statusCode: StatusCodes.Status400BadRequest
                        );
                    }

                    Directory.CreateDirectory(directoryPath);

                    var productList = new List<Product>();

                    if (File.Exists(filePath))
                    {
                        using var streamReader = new StreamReader(filePath);
                        var json = await streamReader.ReadToEndAsync();
                        if (!string.IsNullOrWhiteSpace(json) && json.Trim() != "[]")
                        {
                            productList = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
                        }
                    }

                    productList.Add(newProduct);

                    string jsonToString = JsonSerializer.Serialize<List<Product>>(productList, jsonOptions);
                    using var streamWriter = new StreamWriter(filePath);
                    await streamWriter.WriteAsync(jsonToString);
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
                    Console.WriteLine($"File access error: {ex.Message}");
                    return Results.Problem(
                        detail: $"File access error: {ex.Message}",
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return Results.Problem(
                        detail: $"Unexpected error: {ex.Message}",
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }

                return Results.Created<Product>("", newProduct);
            });

            app.MapPut("/products/{id}", async (int id, UpdateProductDTO updateProduct) =>
            {
                try
                {
                    if (updateProduct == null)
                    {
                        return Results.Problem(
                            detail: "Updated product cannot be null.",
                            statusCode: StatusCodes.Status400BadRequest
                        );
                    }

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

                    if (!string.IsNullOrEmpty(updateProduct.Name))
                        product.Name = updateProduct.Name;
                    if (updateProduct.Price.HasValue)
                        product.Price = updateProduct.Price.Value;
                    if (!string.IsNullOrEmpty(updateProduct.Category))
                        product.Category = updateProduct.Category;

                    string jsonToString = JsonSerializer.Serialize(productList, jsonOptions);

                    using (var streamWriter = new StreamWriter(filePath))
                    {
                        await streamWriter.WriteAsync(jsonToString);
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
            });

            app.MapDelete("/products/{id}", () => "Delete product");

            app.Run();
        }
    }
}
