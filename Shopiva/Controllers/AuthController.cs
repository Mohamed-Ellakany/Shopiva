namespace Shopiva.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest, CancellationToken cancellationToken)
        {
            var result = await _authService.GetTokenAsync(loginRequest.Email, loginRequest.Password, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken)
        {
            var result = await _authService.GetRefreshTokenAsync(refreshTokenRequest.Token, refreshTokenRequest.RefreshToken, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        [HttpPost("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken)
        {
            var result = await _authService.RevokeRefreshTokenAsync(refreshTokenRequest.Token, refreshTokenRequest.RefreshToken, cancellationToken);
            return result.IsSuccess
                ? Ok()
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerRequest, CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterAsync(registerRequest, cancellationToken);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }
    }
}