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
                return Result.Failure<CategoryResponseDto>(UserErrors.CategoryNotFound);

            return Result<CategoryResponseDto>.Success(MapToDto(category));
        }

        public async Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto)
        {
            var nameExists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (nameExists)
                return Result.Failure<CategoryResponseDto>(UserErrors.CategoryAlreadyExists);

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            if (dto.Image is not null)
            {
                var uploadResult = await _imageService.UploadAsync(dto.Image, "categories");
                if (!uploadResult.IsSuccess)
                    return Result.Failure<CategoryResponseDto>(UserErrors.ImageUploadFailed);

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
                return Result.Failure<CategoryResponseDto>(UserErrors.CategoryNotFound);

            // Name uniqueness (exclude self)
            if (dto.Name is not null)
            {
                var nameExists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);

                if (nameExists)
                    return Result.Failure<CategoryResponseDto>(UserErrors.CategoryAlreadyExists);

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
                    return Result.Failure<CategoryResponseDto>(UserErrors.ImageUploadFailed);

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
                return Result.Failure<bool>(UserErrors.CategoryNotFound);

            if (category.Products.Any(p => p.IsActive))
                return Result.Failure<bool>(UserErrors.CategoryHasActiveProducts);

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