using System.ComponentModel.DataAnnotations;

namespace Shopiva.Contracts.Categories
{
    public class UpdateCategoryDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public IFormFile? Image { get; set; }
        public bool RemoveImage { get; set; } = false;
    }
}
