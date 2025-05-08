
using System;
using System.Formats.Asn1;
using System.IO;
using System.Text.Json;
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

            app.MapGet("/products", async () =>
            {
                var products = new List<Product>();
                try
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data Source", "products.json");
                    if (!File.Exists(filePath))
                    {
                        return Results.Problem(
                        detail: $"File {filePath} does not exist",
                        statusCode: StatusCodes.Status404NotFound
                        );

                    }

                    using var streamReader = new StreamReader(filePath);
                    string json = await streamReader.ReadToEndAsync();
                    if (json == "[]")
                    {
                        return Results.Ok(products);
                    }

                    products = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
                    if (products.Count <= 0)
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

                return Results.Ok(products);
            });

            app.MapGet("/products/{id}", () => "Get products by id");
            app.MapPost("/products", async (Product newProduct) =>
            {
                

                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Data Source");
                string filePath = Path.Combine(directoryPath, "products.json");

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

                string objectToString = JsonSerializer.Serialize<List<Product>>(productList, new JsonSerializerOptions { WriteIndented = true });

                using var streamWriter = new StreamWriter(filePath);
                await streamWriter.WriteAsync(objectToString);

                return Results.Created<Product>("", newProduct);
            });
            app.MapPut("/products/{id}", () => "Update product");
            app.MapDelete("/products/{id}", () => "Delete product");

            app.Run();
        }
    }
}
