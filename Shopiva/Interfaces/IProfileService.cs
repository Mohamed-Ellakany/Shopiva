using Shopiva.Contracts.Profiles;

namespace Shopiva.Interfaces
{
    public interface IProfileService
    {
        /// <summary>Get the authenticated user's full profile.</summary>
        Task<Result<ProfileResponseDto>> GetProfileAsync(
            string userId,
            CancellationToken ct = default);

        /// <summary>Update name, phone, and address fields (all optional / patch-style).</summary>
        Task<Result<ProfileResponseDto>> UpdateProfileAsync(
            string userId,
            UpdateProfileDto dto,
            CancellationToken ct = default);

        /// <summary>Change password — verifies current password first.</summary>
        Task<Result<bool>> ChangePasswordAsync(
            string userId,
            ChangePasswordDto dto,
            CancellationToken ct = default);

        /// <summary>Upload a new profile image or remove the existing one.</summary>
        Task<Result<ProfileResponseDto>> UpdateProfileImageAsync(
            string userId,
            UpdateProfileImageDto dto,
            CancellationToken ct = default);
    }
}