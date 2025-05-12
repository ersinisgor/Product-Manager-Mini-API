using Microsoft.AspNetCore.Http;
using Product_Manager_Mini_API.DTOs;
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

            app.MapGet("/products", async (ProductService productService) => await productService.GetAllProductsAsync());

            app.MapGet("/products/{id}", async (int id, ProductService productService) =>
                await productService.GetProductByIdAsync(id));

            app.MapPost("/products", async (ProductService productService, CreateProductDTO newProduct) =>
                await productService.CreateProductAsync(newProduct));

            app.MapPut("/products/{id}", async (int id, UpdateProductDTO updateProduct, ProductService productService) =>
                await productService.UpdateProductAsync(id, updateProduct));

            app.MapDelete("/products/{id}", async (int id, ProductService productService) =>
                await productService.DeleteProductAsync(id));

            app.Run();
        }
    }
}
