
using System;
using System.Formats.Asn1;
using System.IO;
using System.Text.Json;
using Product_Manager_Mini_API.Models;
using Product_Manager_Mini_API.Services;

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


                    using var streamReader = new StreamReader(filePath);
                    var json = await streamReader.ReadToEndAsync();
                    Console.WriteLine($"json: {json}");

                    products = JsonSerializer.Deserialize<List<Product>>(json);

                    foreach (var product in products)
                    {
                        Console.WriteLine($"{JsonSerializer.Serialize(product)}");
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                return products;
            });

            app.MapGet("/products/{id}", () => "Get products by id");
            app.MapPost("/products/{id}", () => "Create product");
            app.MapPut("/products/{id}", () => "Update product");
            app.MapDelete("/products/{id}", () => "Delete product");

            app.Run();
        }
    }
}
