namespace Shopiva.Interfaces
{
    public interface IImageService
    {
        Task<Result<string>> UploadAsync(IFormFile file, string folder = "products");
        Task<Result<bool>> DeleteAsync(string url);
        Task<Result<List<string>>> UploadManyAsync(List<IFormFile> files, string folder = "products");
    }
}
