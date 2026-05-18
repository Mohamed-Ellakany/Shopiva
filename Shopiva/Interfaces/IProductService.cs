using Shopiva.Contracts.Products;
using Shopiva.Interfaces;

namespace Shopiva.Interfaces
{
    public interface IProductService
    {
        Task<Result<PaginatedResult<ProductResponseDto>>> GetAllAsync(ProductFilterDto filter);
        Task<Result<ProductResponseDto>> GetByIdAsync(int id);
        Task<Result<ProductResponseDto>> CreateAsync(CreateProductDto dto, string sellerId);
        Task<Result<ProductResponseDto>> UpdateAsync(int id, UpdateProductDto dto, string sellerId);
        Task<Result<bool>> DeleteAsync(int id, string sellerId, bool isAdmin);
        Task<Result<bool>> UpdateStockAsync(int id, int quantity); 
    }
}

