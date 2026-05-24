
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
        /// <summary>
        /// Step 1 – request a 6-digit OTP. Always returns 200 (anti-enumeration).
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            await _authService.ForgotPasswordAsync(request.Email, cancellationToken);
            return Ok(new { message = "If an account with that email exists, a 6-digit code has been sent." });
        }

        /// <summary>
        /// Step 2 – submit OTP + new password to complete the reset.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword, cancellationToken);
            return result.IsSuccess
                ? Ok(new { message = "Password has been reset successfully." })
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

       

        /// <summary>
        /// (Re)send a 6-digit email-confirmation OTP. Always returns 200 (anti-enumeration).
        /// </summary>
        [HttpPost("send-email-otp")]
        public async Task<IActionResult> SendEmailOtpAsync([FromBody] SendEmailOtpRequest request, CancellationToken cancellationToken)
        {
            await _authService.SendEmailConfirmationOtpAsync(request.Email, cancellationToken);
            return Ok(new { message = "If an unconfirmed account with that email exists, a 6-digit code has been sent." });
        }

        /// <summary>
        /// Verify the email-confirmation OTP.
        /// </summary>
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] VerifyEmailOtpRequest request, CancellationToken cancellationToken)
        {
            var result = await _authService.ConfirmEmailAsync(request.Email, request.Otp, cancellationToken);
            return result.IsSuccess
                ? Ok(new { message = "Email confirmed successfully." })
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }
    }
}
   