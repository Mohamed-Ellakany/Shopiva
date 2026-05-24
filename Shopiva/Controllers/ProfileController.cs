using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shopiva.Contracts.Profiles;

namespace Shopiva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController(IProfileService profileService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>Get the authenticated user's profile.</summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var result = await profileService.GetProfileAsync(UserId, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// Update name, phone number, and address.
        /// All fields are optional — only the provided fields are updated.
        /// </summary>
        [HttpPatch]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileDto dto, CancellationToken ct)
        {
            var result = await profileService.UpdateProfileAsync(UserId, dto, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Change password.
        /// Requires the current password. Revokes all active refresh tokens on success.
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto, CancellationToken ct)
        {
            var result = await profileService.ChangePasswordAsync(UserId, dto, ct);
            return result.IsSuccess
                ? Ok(new { message = "Password changed successfully. Please log in again on other devices." })
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Upload a new profile image or remove the existing one.
        /// Send as multipart/form-data. Set RemoveImage=true to remove without uploading.
        /// </summary>
        [HttpPatch("image")]
        public async Task<IActionResult> UpdateProfileImage(
            [FromForm] UpdateProfileImageDto dto, CancellationToken ct)
        {
            var result = await profileService.UpdateProfileImageAsync(UserId, dto, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }
    }
}
