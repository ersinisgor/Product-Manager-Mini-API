namespace Product_Manager_Mini_API.DTOs
{
    public class CreateProductDTO
    {
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required string Category { get; set; }
    }
}
