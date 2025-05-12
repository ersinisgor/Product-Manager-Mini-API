using System.ComponentModel.DataAnnotations;

namespace Product_Manager_Mini_API.DTOs
{
    public class CreateProductDTO
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public required string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public required decimal Price { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
        public required string Category { get; set; }
    }
}
