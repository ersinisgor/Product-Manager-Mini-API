using System.Text.Json.Serialization;

namespace Product_Manager_Mini_API.Models
{
    public class Product
    {
        [JsonPropertyName("id")]
        public required int Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("price")]
        public required decimal Price { get; set; }
        [JsonPropertyName("category")]
        public required string Category { get; set; }
    }
}
