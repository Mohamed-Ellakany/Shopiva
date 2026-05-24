namespace Shopiva.Contracts.Profiles
{
    // ── Responses ─────────────────────────────────────────────────────────────

    public class ProfileResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string> Roles { get; set; } = [];
    }

    // ── Requests ─────────────────────────────────────────────────────────────

    /// <summary>Update display name and address fields.</summary>
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }

    /// <summary>Change password — requires current password for verification.</summary>
    public record ChangePasswordDto(
        string CurrentPassword,
        string NewPassword,
        string ConfirmNewPassword
    );

    /// <summary>Upload or remove the profile picture.</summary>
    public class UpdateProfileImageDto
    {
        public IFormFile? Image { get; set; }
        public bool RemoveImage { get; set; } = false;
    }
}