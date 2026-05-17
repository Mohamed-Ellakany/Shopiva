
using System.Security.Cryptography;

namespace Shopiva.Services
{
    public class AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtProvider jwtProvider) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IJwtProvider _jwtProvider = jwtProvider;

        private readonly int _refreshTokenExpirationDays = 14;


        public async Task<Result<AuthResponse>> getTokenAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            //check user ? 
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            
            var result = await _signInManager.PasswordSignInAsync(user, password, false , false) ;

            if (result.Succeeded)
            {
                var response = await GetToken(user);
                return Result.Success(response);
            }

            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        }



        public async Task<Result<AuthResponse>> getRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null) return Result.Failure<AuthResponse>(UserErrors.InvalidToken);

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null) return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;

            var response = await GetToken(user);

            return Result.Success( response);

        }

        public async Task<Result<bool>> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = _jwtProvider.ValidateToken(token);

            if (userId is null) return Result.Failure<bool>(UserErrors.InvalidToken);

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return Result.Failure<bool>(UserErrors.InvalidCredentials); ;

            var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

            if (userRefreshToken is null) return Result.Failure<bool>(UserErrors.InvalidRefreshToken);

            userRefreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return Result.Success(true); 
        }

        public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest registerRequest, CancellationToken cancellationToken = default)
        {
            var emailIsExist = await _userManager.Users.AnyAsync(x => x.Email == registerRequest.Email);
            if (emailIsExist) return Result.Failure<AuthResponse>(UserErrors.EmailAlreadyInUse);

            var user = new ApplicationUser
            {
                UserName = registerRequest.Email,
                Email = registerRequest.Email,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName
            };


            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (result.Succeeded)
            {
                 var response = await GetToken(user);
                 return Result.Success( response);
            }


            return Result.Failure<AuthResponse>(UserErrors.InvalidOperation);

        }


        private async Task<AuthResponse> GetToken(ApplicationUser user)
        {
            
            var (token, expireIn) =await _jwtProvider.GenerateJwtTokenAsync(user);

            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays);

            user.RefreshTokens.Add(new RefreshTokens
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiration
            });

            await _userManager.UpdateAsync(user);

            var response = new AuthResponse(user.Id, user.Email, user.FirstName, user.LastName, token, expireIn, refreshToken, refreshTokenExpiration);

            return response;

          
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        }


    }
}





