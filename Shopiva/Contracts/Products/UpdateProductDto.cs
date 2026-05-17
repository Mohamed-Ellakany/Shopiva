using System.ComponentModel.DataAnnotations;

namespace Shopiva.Contracts.Products
{
    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)] 
        public decimal? Price { get; set; }
        public decimal? DiscountedPrice { get; set; }

        [Range(0, int.MaxValue)] 
        public int? Stock { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public List<IFormFile>? NewImages { get; set; }
        public List<int>? RemoveImageIds { get; set; }

    }
}
