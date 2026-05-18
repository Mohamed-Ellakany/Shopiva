using System.ComponentModel.DataAnnotations;

namespace Shopiva.Contracts.Products
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required] 
        public string Description { get; set; } = string.Empty;
        
        [Range(0.01, double.MaxValue)]
        [Required]
        public decimal Price { get; set; }
        
        public decimal? DiscountedPrice { get; set; }
        
        [Range(0, int.MaxValue)]
        [Required]
        public int Stock { get; set; }
        
        [Required] 
        public int CategoryId { get; set; }
        public List<IFormFile>? Images { get; set; }


    }
}
