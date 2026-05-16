using Microsoft.AspNetCore.Mvc;

namespace Shopiva.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            var authResult = await _authService.getTokenAsync(loginRequest.Email, loginRequest.Password, cancellationToken);

            return authResult.IsSuccess ? Ok(authResult.Value)
                : authResult.ToProblem(StatusCodes.Status400BadRequest);

        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody]RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken)
        {
            var authResult = await _authService.getRefreshTokenAsync(refreshTokenRequest.Token, refreshTokenRequest.RefreshToken, cancellationToken);

            return authResult.IsSuccess ?
                 Ok(authResult.Value) :
                authResult.ToProblem(StatusCodes.Status400BadRequest);
        }

        [HttpPost("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody]RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken)
        {
            var isRevoked = await _authService.RevokeRefreshTokenAsync(refreshTokenRequest.Token, refreshTokenRequest.RefreshToken, cancellationToken);

            return isRevoked.IsSuccess ? Ok()
                : isRevoked.ToProblem(StatusCodes.Status400BadRequest);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest, CancellationToken cancellationToken)
        {
            var response = await _authService.RegisterAsync(registerRequest , cancellationToken);

            return response.IsSuccess ?
                 Ok(response.Value) :
                response.ToProblem(StatusCodes.Status400BadRequest);
        }

    }

}
