
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

            app.MapPut("/products/{id}", async (ProductService productService, int id, UpdateProductDTO updateProduct) => await productService.UpdateProductAsync(id, updateProduct));

            app.MapDelete("/products/{id}", async (ProductService productService, int id) => await productService.DeleteProductAsync(id));

            app.Run();
        }
    }
}
