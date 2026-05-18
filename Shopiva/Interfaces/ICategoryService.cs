using Shopiva.Contracts.Categories;

namespace Shopiva.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<List<CategoryResponseDto>>> GetAllAsync();
        Task<Result<CategoryResponseDto>> GetByIdAsync(int id);
        Task<Result<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto);
        Task<Result<CategoryResponseDto>> UpdateAsync(int id, UpdateCategoryDto dto);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
