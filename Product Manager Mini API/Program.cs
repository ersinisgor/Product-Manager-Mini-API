
namespace Product_Manager_Mini_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
       
            app.MapGet("/products", () => "Get all products");
            app.MapGet("/products/{id}", () => "Get products by id");
            app.MapPost("/products/{id}", () => "Create product");
            app.MapPut("/products/{id}", () => "Update product");
            app.MapDelete("/products/{id}", () => "Delete product");

            app.Run();
        }
    }
}
