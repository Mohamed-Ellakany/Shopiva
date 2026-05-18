namespace Shopiva.Services
{
    public class ImageService : IImageService
    {
        private readonly string _uploadsPath;
        private readonly string _baseUrl;

        public ImageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _uploadsPath = Path.Combine(env.WebRootPath, "uploads");

            Directory.CreateDirectory(Path.Combine(_uploadsPath, "products"));
            Directory.CreateDirectory(Path.Combine(_uploadsPath, "categories"));

            var request = httpContextAccessor.HttpContext!.Request;
            _baseUrl = $"{request.Scheme}://{request.Host}";

        }

        public async Task<Result<string>> UploadAsync(IFormFile file, string folder = "products")
        {
            var validationError = ValidateFile(file);
            if (validationError is not null)
                return Result.Failure<string>(new Error("Invalid File", validationError));

            var fileName = GenerateFileName(file.FileName);
            var folderPath = Path.Combine(_uploadsPath, folder);
            var filePath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"{_baseUrl}/uploads/{folder}/{fileName}";
            return Result<string>.Success(url);
        }

        public async Task<Result<List<string>>> UploadManyAsync(List<IFormFile> files, string folder = "products")
        {
            if (files.Count == 0)
                return Result.Failure<List<string>>(new Error("No Images", "No files provided"));

            if (files.Count > 10)
                return Result.Failure<List<string>>(new Error("Max Images","Maximum 10 images allowed"));

            var urls = new List<string>();

            foreach (var file in files)
            {
                var result = await UploadAsync(file, folder);
                if (!result.IsSuccess)
                    return Result.Failure<List<string>>( result.Error!);

                urls.Add(result.Value!);
            }

            return Result<List<string>>.Success(urls);
        }

        public Task<Result<bool>> DeleteAsync(string url)
        {
            try
            {
                var filePath = UrlToFilePath(url);

                if (filePath is null || !File.Exists(filePath))
                    return Task.FromResult(Result.Failure<bool>(new Error("File not found", "File not found")));

                File.Delete(filePath);
                return Task.FromResult(Result<bool>.Success(true));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result.Failure<bool>(new Error("Failed to delete file", $"Failed to delete file: {ex.Message}")));
            }
        }

        // ──────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────

        private static string? ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
                return "File is empty";

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return "Only JPEG, PNG, and WebP images are allowed";

            const long maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize)
                return "Image must be less than 5MB";

            return null;
        }

        private static string GenerateFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLower();
            return $"{Guid.NewGuid()}{extension}";
        }

        // Converts "https://localhost:5001/uploads/products/abc.jpg"
        //       → "C:/project/wwwroot/uploads/products/abc.jpg"
        private string? UrlToFilePath(string url)
        {
            try
            {
                var uri = new Uri(url);
                // uri.AbsolutePath → "/uploads/products/abc.jpg"
                var relativePath = uri.AbsolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

                // relativePath starts with "uploads/..." — map to wwwroot
                var wwwrootParent = Directory.GetParent(_uploadsPath)!.FullName;
                return Path.Combine(wwwrootParent, relativePath);
            }
            catch
            {
                return null;
            }
        }
    }
}
