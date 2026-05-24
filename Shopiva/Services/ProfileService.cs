
namespace Shopiva.Services
{
     public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageService _imageService;

        public ProfileService(
            UserManager<ApplicationUser> userManager,
            IImageService imageService)
        {
            _userManager = userManager;
            _imageService = imageService;
        }

        // ── Get Profile ───────────────────────────────────────────────────────

        public async Task<Result<ProfileResponseDto>> GetProfileAsync(
            string userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<ProfileResponseDto>(UserErrors.InvalidCredentials);

            return Result.Success(await MapToDtoAsync(user));
        }

        // ── Update Profile Fields ─────────────────────────────────────────────

        public async Task<Result<ProfileResponseDto>> UpdateProfileAsync(
            string userId, UpdateProfileDto dto, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<ProfileResponseDto>(UserErrors.InvalidCredentials);

            // Apply only the fields that were provided (patch semantics)
            if (dto.FirstName is not null) user.FirstName = dto.FirstName;
            if (dto.LastName is not null) user.LastName = dto.LastName;
            if (dto.Address is not null) user.Address = dto.Address;
            if (dto.City is not null) user.City = dto.City;
            if (dto.Country is not null) user.Country = dto.Country;

            // Phone number change — must stay unique across users
            if (dto.PhoneNumber is not null && dto.PhoneNumber != user.PhoneNumber)
            {
                var phoneInUse = _userManager.Users
                    .Any(u => u.PhoneNumber == dto.PhoneNumber && u.Id != userId);

                if (phoneInUse)
                    return Result.Failure<ProfileResponseDto>(UserErrors.PhoneAlreadyInUse);

                user.PhoneNumber = dto.PhoneNumber;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result.Failure<ProfileResponseDto>(new Error(
                    "Profile.UpdateFailed",
                    string.Join("; ", result.Errors.Select(e => e.Description))));

            return Result.Success(await MapToDtoAsync(user));
        }

        // ── Change Password ───────────────────────────────────────────────────

        public async Task<Result<bool>> ChangePasswordAsync(
            string userId, ChangePasswordDto dto, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<bool>(UserErrors.InvalidCredentials);

            // Verify current password before allowing the change
            var passwordCorrect = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!passwordCorrect)
                return Result.Failure<bool>(UserErrors.WrongCurrentPassword);

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return Result.Failure<bool>(new Error(
                    "Password.ChangeFailed",
                    string.Join("; ", result.Errors.Select(e => e.Description))));

            // Revoke all existing refresh tokens so the user must re-login on other devices
            foreach (var rt in user.RefreshTokens.Where(r => r.IsActive))
                rt.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Result.Success(true);
        }

        // ── Update Profile Image ──────────────────────────────────────────────

        public async Task<Result<ProfileResponseDto>> UpdateProfileImageAsync(
            string userId, UpdateProfileImageDto dto, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<ProfileResponseDto>(UserErrors.InvalidCredentials);

            // Remove existing image first (for both remove and replace flows)
            if (user.ProfileImageUrl is not null && (dto.RemoveImage || dto.Image is not null))
            {
                await _imageService.DeleteAsync(user.ProfileImageUrl);
                user.ProfileImageUrl = null;
            }

            // Upload new image if provided
            if (dto.Image is not null)
            {
                var uploadResult = await _imageService.UploadAsync(dto.Image, "avatars");
                if (!uploadResult.IsSuccess)
                    return Result.Failure<ProfileResponseDto>(UserErrors.ImageUploadFailed);

                user.ProfileImageUrl = uploadResult.Value;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Failure<ProfileResponseDto>(new Error(
                    "Profile.UpdateFailed",
                    string.Join("; ", updateResult.Errors.Select(e => e.Description))));

            return Result.Success(await MapToDtoAsync(user));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task<ProfileResponseDto> MapToDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new ProfileResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                ProfileImageUrl = user.ProfileImageUrl,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            };
        }
    }
}