using Org.BouncyCastle.Crypto.Generators;
using Shopiva.Interfaces.Redis;
using System.Security.Cryptography;

namespace Shopiva.Services
{
    public class AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtProvider jwtProvider,
        IEmailService emailService,
        IRedisService redisService) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;
        private readonly IEmailService _emailService = emailService;
        private readonly IRedisService _redisService = redisService;

        private readonly int _refreshTokenExpirationDays = 14;
        private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(10);

        private static string OtpKey(string purpose, string userId) => $"otp:{purpose}:{userId}";

        public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            var response = await GetTokenAsync(user);
            return Result.Success(response);
        }

        public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtProvider.ValidateToken(token);
            if (userId is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
            if (userRefreshToken is null)
                return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var response = await GetTokenAsync(user);
            return Result.Success(response);
        }

        public async Task<Result<bool>> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtProvider.ValidateToken(token);
            if (userId is null)
                return Result.Failure<bool>(UserErrors.InvalidToken);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<bool>(UserErrors.InvalidCredentials);

            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);
            if (userRefreshToken is null)
                return Result.Failure<bool>(UserErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Result.Success(true);
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest registerRequest, CancellationToken cancellationToken = default)
        {
            var emailExists = await _userManager.Users.AnyAsync(x => x.Email == registerRequest.Email);
            if (emailExists)
                return Result.Failure<AuthResponse>(UserErrors.EmailAlreadyInUse);

            var user = new ApplicationUser
            {
                UserName = registerRequest.Email,
                Email = registerRequest.Email,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName
            };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!result.Succeeded)
                return Result.Failure<AuthResponse>(UserErrors.InvalidOperation);

            await _userManager.AddToRoleAsync(user, "Customer");

            await IssueOtpAsync(user, OtpPurpose.EmailConfirmation, cancellationToken);

            var response = await GetTokenAsync(user);
            return Result.Success(response);
        }


        private async Task<AuthResponse> GetTokenAsync(ApplicationUser user)
        {
            var (token, expireIn) = await _jwtProvider.GenerateJwtTokenAsync(user);

            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

            user.RefreshTokens.Add(new RefreshTokens
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration
            });

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                token,
                expireIn,
                refreshToken,
                refreshTokenExpiration,
                roles.ToList()
            );
        }

        private static string GenerateRefreshToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        public async Task<Result<bool>> ForgotPasswordAsync(
               string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Always return success — never reveal whether email exists
            if (user is null) return Result.Success(true);

            await IssueOtpAsync(user, OtpPurpose.PasswordReset, cancellationToken);
            return Result.Success(true);
        }



        public async Task<Result<bool>> ResetPasswordAsync(
            string email, string otp, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return Result.Failure<bool>(UserErrors.InvalidCredentials);

            var verifyResult = await VerifyOtpAsync(user.Id, otp, OtpPurpose.PasswordReset);
            if (!verifyResult.IsSuccess) return verifyResult;

            // Replace password
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
                return Result.Failure<bool>(new Error("Password.ResetFailed",
                    string.Join("; ", removeResult.Errors.Select(e => e.Description))));

            var addResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addResult.Succeeded)
                return Result.Failure<bool>(new Error("Password.ResetFailed",
                    string.Join("; ", addResult.Errors.Select(e => e.Description))));

            // Delete OTP from Redis and revoke all refresh tokens (security)
            await _redisService.DeleteAsync(OtpKey(OtpPurpose.PasswordReset, user.Id));

            foreach (var rt in user.RefreshTokens.Where(r => r.IsActive))
                rt.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            return Result.Success(true);
        }

        public async Task<Result<bool>> SendEmailConfirmationOtpAsync(
           string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Always succeed — anti-enumeration
            if (user is null) return Result.Success(true);

            if (user.EmailConfirmed)
                return Result.Failure<bool>(new Error("Email.AlreadyConfirmed", "Email is already confirmed."));

            await IssueOtpAsync(user, OtpPurpose.EmailConfirmation, cancellationToken);
            return Result.Success(true);
        }

        public async Task<Result<bool>> ConfirmEmailAsync(
            string email, string otp, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return Result.Failure<bool>(UserErrors.InvalidCredentials);

            if (user.EmailConfirmed)
                return Result.Failure<bool>(new Error("Email.AlreadyConfirmed", "Email is already confirmed."));

            var verifyResult = await VerifyOtpAsync(user.Id, otp, OtpPurpose.EmailConfirmation);
            if (!verifyResult.IsSuccess) return verifyResult;

            // Mark email confirmed via Identity
            var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmResult = await _userManager.ConfirmEmailAsync(user, confirmToken);
            if (!confirmResult.Succeeded)
                return Result.Failure<bool>(new Error("Email.ConfirmFailed",
                    string.Join("; ", confirmResult.Errors.Select(e => e.Description))));

            // Delete OTP from Redis — no longer needed
            await _redisService.DeleteAsync(OtpKey(OtpPurpose.EmailConfirmation, user.Id));

            return Result.Success(true);
        }

        //----
        private async Task IssueOtpAsync(
            ApplicationUser user, string purpose, CancellationToken cancellationToken)
        {
            var plainOtp = GenerateOtp();

            var entry = new OtpEntry
            {
                OtpHash = BCrypt.Net.BCrypt.HashPassword(plainOtp),
                Attempts = 0
            };

            // Overwrite any existing OTP for this purpose (covers "resend" case)
            await _redisService.SetAsync(OtpKey(purpose, user.Id), entry, OtpTtl);

            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(displayName)) displayName = user.Email!;

            if (purpose == OtpPurpose.PasswordReset)
                await _emailService.SendPasswordResetOtpAsync(user.Email!, displayName, plainOtp, cancellationToken);
            else
                await _emailService.SendEmailConfirmationOtpAsync(user.Email!, displayName, plainOtp, cancellationToken);
        }




        private async Task<Result<bool>> VerifyOtpAsync(string userId, string plainOtp, string purpose)
        {
            var key = OtpKey(purpose, userId);
            var cacheResult = await _redisService.GetAsync<OtpEntry>(key);

            // Missing → never issued or already expired by Redis TTL
            if (!cacheResult.IsSuccess || cacheResult.Value is null)
                return Result.Failure<bool>(UserErrors.OtpInvalidOrExpired);

            var entry = cacheResult.Value;

            // Block brute-force
            if (entry.Attempts >= OtpEntry.MaxAttempts)
            {
                await _redisService.DeleteAsync(key);
                return Result.Failure<bool>(UserErrors.OtpTooManyAttempts);
            }

            if (!BCrypt.Net.BCrypt.Verify(plainOtp, entry.OtpHash))
            {
                // Increment attempts and write back (keeping the same TTL is not possible
                // via IRedisService, so we rewrite with a fresh 10-min window per attempt).
                entry.Attempts++;
                await _redisService.SetAsync(key, entry, OtpTtl);
                return Result.Failure<bool>(UserErrors.OtpInvalidOrExpired);
            }

            return Result.Success(true);
        }

        private static string GenerateOtp()
            => RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

       






        public static class OtpPurpose
        {
            public const string PasswordReset = "PasswordReset";
            public const string EmailConfirmation = "EmailConfirmation";
        }





    }
}