using Shopiva.Contracts.Categories;

namespace Shopiva.Services
{
    public class CategoryService(AppDbContext context, IImageService imageService) : ICategoryService
    {
        private readonly AppDbContext _context = context;
        private readonly IImageService _imageService = imageService;
        public async Task<Result<List<CategoryResponseDto>>> GetAllAsync()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    ProductCount = c.Products.Count(p => p.IsActive),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Result<List<CategoryResponseDto>>.Success(categories);
        }

        public async Task<Result<CategoryResponseDto>> GetByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
                return Result.Failure<CategoryResponseDto>(new Error("Category Not Found", "Category not found"));

            return Result<CategoryResponseDto>.Success(MapToDto(category));
        }

        public async Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto)
        {
            var nameExists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
                return Result.Failure<CategoryResponseDto>(new Error("Category Already Exists", "A category with this name already exists"));

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            if (dto.Image is not null)
            {
                var uploadResult = await _imageService.UploadAsync(dto.Image, "categories");
                if (!uploadResult.IsSuccess)
                    return Result.Failure<CategoryResponseDto>(new Error("Image Upload Failed", "Failed to upload category image"));

                category.ImageUrl = uploadResult.Value;
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Result<CategoryResponseDto>.Success(MapToDto(category));
        }

        public async Task<Result<CategoryResponseDto>> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
                return Result.Failure<CategoryResponseDto>(new Error("Category Not Found", "Category not found"));

            // Name uniqueness (exclude self)
            if (dto.Name is not null)
            {
                var nameExists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);

                if (nameExists)
                    return Result.Failure<CategoryResponseDto>(new Error("Category Already Exists", "A category with this name already exists"));

                category.Name = dto.Name;
            }

            if (dto.Description is not null)
                category.Description = dto.Description;

            // Remove image
            if (dto.RemoveImage && category.ImageUrl is not null)
            {
                await _imageService.DeleteAsync(category.ImageUrl);
                category.ImageUrl = null;
            }
            // Replace image
            else if (dto.Image is not null)
            {
                if (category.ImageUrl is not null)
                    await _imageService.DeleteAsync(category.ImageUrl);

                var uploadResult = await _imageService.UploadAsync(dto.Image, "categories");
                if (!uploadResult.IsSuccess)
                    return Result.Failure<CategoryResponseDto>(new Error("Image Upload Failed", "Failed to upload category image"));

                category.ImageUrl = uploadResult.Value;
            }

            await _context.SaveChangesAsync();

            return Result<CategoryResponseDto>.Success(MapToDto(category));
        }

        public async Task<Result<bool>> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
                return Result.Failure<bool>(new Error("Category Not Found", "Category not found"));

            if (category.Products.Any(p => p.IsActive))
                return Result.Failure<bool>(new Error("Cannot Delete Category", "Cannot delete a category that has active products. Reassign or remove them first"));

            // Delete image from wwwroot if exists
            if (category.ImageUrl is not null)
                await _imageService.DeleteAsync(category.ImageUrl);

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        private static CategoryResponseDto MapToDto(Category c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ImageUrl = c.ImageUrl,
            ProductCount = c.Products.Count(p => p.IsActive),
            CreatedAt = c.CreatedAt
        };
    }
}