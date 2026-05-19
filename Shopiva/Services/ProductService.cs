

public class ProductService(AppDbContext context, IImageService imageService) : IProductService
{
    private readonly AppDbContext _context = context;
    private readonly IImageService _imageService = imageService;
    public async Task<Result<PaginatedResult<ProductResponseDto>>> GetAllAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Seller)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p =>
                p.Name.Contains(filter.Search) ||
                p.Description.Contains(filter.Search));

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice);

        if (filter.InStock == true)
            query = query.Where(p => p.Stock > 0);

        query = filter.SortBy.ToLower() switch
        {
            "price" => filter.Descending
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "name" => filter.Descending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            _ => filter.Descending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountedPrice = p.DiscountedPrice,
                Stock = p.Stock,
                IsActive = p.IsActive,
                CategoryName = p.Category.Name,
                SellerName = p.Seller.UserName ?? "",
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                ImageUrls = p.Images.Select(i => i.Url).ToList(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return Result<PaginatedResult<ProductResponseDto>>.Success(new PaginatedResult<ProductResponseDto>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<Result<ProductResponseDto>> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product is null)
            return Result.Failure<ProductResponseDto>(UserErrors.ProductNotFound);

        return Result<ProductResponseDto>.Success(MapToDto(product));
    }

    public async Task<Result<ProductResponseDto>> CreateAsync(CreateProductDto dto, string sellerId)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
            return Result.Failure<ProductResponseDto>(UserErrors.CategoryNotFound);

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DiscountedPrice = dto.DiscountedPrice,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            SellerId = sellerId
        };

        if (dto.Images != null)
        {
            var isFirst = true;
            foreach (var image in dto.Images)
            {
                var uploadedResult = await _imageService.UploadAsync(image);
                if (!uploadedResult.IsSuccess)
                    return Result.Failure<ProductResponseDto>(UserErrors.ImageUploadFailed);

                product.Images.Add(new ProductImage { Url = uploadedResult.Value, IsMain = isFirst });
                isFirst = false;
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        await _context.Entry(product).Reference(p => p.Seller).LoadAsync();

        return Result<ProductResponseDto>.Success(MapToDto(product));
    }

    public async Task<Result<ProductResponseDto>> UpdateAsync(int id, UpdateProductDto dto, string sellerId)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return Result.Failure<ProductResponseDto>(UserErrors.ProductNotFound);

        if (product.SellerId != sellerId)
            return Result.Failure<ProductResponseDto>(UserErrors.UnauthorizedAccess);

        if (dto.Name is not null) product.Name = dto.Name;
        if (dto.Description is not null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.DiscountedPrice.HasValue) product.DiscountedPrice = dto.DiscountedPrice;
        if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;
        if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;

        // Remove images
        if (dto.RemoveImageIds?.Any() == true)
        {
            var toRemove = product.Images.Where(i => dto.RemoveImageIds.Contains(i.Id)).ToList();
            foreach (var img in toRemove)
            {
                await _imageService.DeleteAsync(img.Url);
                product.Images.Remove(img);
            }
        }

        // Add new images
        if (dto.NewImages?.Any() == true)
        {
            foreach (var image in dto.NewImages)
            {
                var uploadedResult = await _imageService.UploadAsync(image);
                if (!uploadedResult.IsSuccess)
                    return Result.Failure<ProductResponseDto>(UserErrors.ImageUploadFailed);

                product.Images.Add(new ProductImage
                {
                    Url = uploadedResult.Value,
                    IsMain = !product.Images.Any()
                });
            }
        }

        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result.Success(MapToDto(product));
    }

    public async Task<Result<bool>> DeleteAsync(int id, string sellerId, bool isAdmin)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return Result.Failure<bool>(UserErrors.ProductNotFound);

        if (!isAdmin && product.SellerId != sellerId)
            return Result.Failure<bool>(UserErrors.UnauthorizedAccess);

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> UpdateStockAsync(int id, int quantity)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
            return Result.Failure<bool>(UserErrors.ProductNotFound);

        if (product.Stock < quantity)
            return Result.Failure<bool>(UserErrors.InsufficientStock);

        product.Stock -= quantity;
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static ProductResponseDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        DiscountedPrice = p.DiscountedPrice,
        Stock = p.Stock,
        IsActive = p.IsActive,
        CategoryName = p.Category?.Name ?? "",
        SellerName = p.Seller?.UserName ?? "",
        AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
        ImageUrls = p.Images.Select(i => i.Url).ToList(),
        CreatedAt = p.CreatedAt
    };
}