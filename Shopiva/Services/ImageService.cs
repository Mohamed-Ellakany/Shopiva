namespace Shopiva.Services
{
    public class ImageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : IImageService
    {
        private readonly string _uploadsPath = Path.Combine(env.WebRootPath, "uploads");
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  
        private string BaseUrl
        {
            get
            {
                var request = _httpContextAccessor.HttpContext!.Request;
                return $"{request.Scheme}://{request.Host}";
            }
        }

        public ImageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, bool _)
            : this(env, httpContextAccessor)
        {
            Directory.CreateDirectory(Path.Combine(_uploadsPath, "products"));
            Directory.CreateDirectory(Path.Combine(_uploadsPath, "categories"));
        }

        private void EnsureDirectories()
        {
            Directory.CreateDirectory(Path.Combine(_uploadsPath, "products"));
            Directory.CreateDirectory(Path.Combine(_uploadsPath, "categories"));
        }

        public async Task<Result<string>> UploadAsync(IFormFile file, string folder = "products")
        {
            EnsureDirectories();

            var validationError = ValidateFile(file);
            if (validationError is not null)
                return Result.Failure<string>(new Error("InvalidFile", validationError));

            var fileName = GenerateFileName(file.FileName);
            var filePath = Path.Combine(_uploadsPath, folder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"{BaseUrl}/uploads/{folder}/{fileName}";
            return Result<string>.Success(url);
        }

        public async Task<Result<List<string>>> UploadManyAsync(List<IFormFile> files, string folder = "products")
        {
            if (files.Count == 0)
                return Result.Failure<List<string>>(UserErrors.NoImages);

            if (files.Count > 10)
                return Result.Failure<List<string>>(UserErrors.MaxImages);

            var urls = new List<string>();
            foreach (var file in files)
            {
                var result = await UploadAsync(file, folder);
                if (!result.IsSuccess)
                    return Result.Failure<List<string>>(result.Error!);

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
                    return Task.FromResult(Result.Failure<bool>(UserErrors.ImageUploadFailed));

                File.Delete(filePath);
                return Task.FromResult(Result<bool>.Success(true));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Result.Failure<bool>(new Error("DeleteFailed", $"Failed to delete file: {ex.Message}")));
            }
        }

      
        private static string? ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
                return "File is empty";

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return "Only JPEG, PNG, and WebP images are allowed";

            const long maxSize = 5 * 1024 * 1024;
            if (file.Length > maxSize)
                return "Image must be less than 5MB";

            return null;
        }

        private static string GenerateFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName).ToLower();
            return $"{Guid.NewGuid()}{extension}";
        }

        private string? UrlToFilePath(string url)
        {
            try
            {
                var uri = new Uri(url);
                var relativePath = uri.AbsolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
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