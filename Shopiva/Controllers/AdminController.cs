namespace Shopiva.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController(IAdminService adminService) : ControllerBase
    {
        // ╔══════════════════════════════════════════════════════════════════════
        // ║  DASHBOARD
        // ╚══════════════════════════════════════════════════════════════════════

        /// <summary>Get platform-wide statistics for the admin dashboard.</summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            var result = await adminService.GetDashboardAsync(ct);
            return Ok(result.Value);
        }

        // ╔══════════════════════════════════════════════════════════════════════
        // ║  USER MANAGEMENT
        // ╚══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// List all users with optional search, role, and restriction filters.
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] UserFilterDto filter, CancellationToken ct)
        {
            var result = await adminService.GetUsersAsync(filter, ct);
            return Ok(result.Value);
        }

        /// <summary>Get full details for a single user including order and review counts.</summary>
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUser(string userId, CancellationToken ct)
        {
            var result = await adminService.GetUserByIdAsync(userId, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// Restrict (lock out) or unrestrict a user account.
        /// Restricted users cannot log in.
        /// Pass Restrict=true with an optional Until date (permanent if omitted).
        /// Pass Restrict=false to lift the restriction immediately.
        /// </summary>
        [HttpPatch("users/{userId}/restrict")]
        public async Task<IActionResult> RestrictUser(
            string userId,
            [FromBody] RestrictUserRequest request,
            CancellationToken ct)
        {
            var result = await adminService.RestrictUserAsync(userId, request, ct);
            return result.IsSuccess
                ? Ok(new { message = request.Restrict ? "User restricted." : "Restriction lifted." })
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Soft-delete a user account.
        /// The account is permanently locked and all PII is anonymised.
        /// Orders, reviews, and products are preserved for data integrity.
        /// Admin accounts cannot be deleted via this endpoint.
        /// </summary>
        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId, CancellationToken ct)
        {
            var result = await adminService.DeleteUserAsync(userId, ct);
            return result.IsSuccess
                ? NoContent()
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Assign a role (Admin | Customer | Seller) to a user.
        /// A user can hold multiple roles simultaneously.
        /// </summary>
        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> AssignRole(
            string userId,
            [FromBody] AssignRoleRequest request,
            CancellationToken ct)
        {
            var result = await adminService.AssignRoleAsync(userId, request, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Revoke a role from a user.
        /// Removing the Admin role is blocked if they are the last admin.
        /// </summary>
        [HttpDelete("users/{userId}/roles")]
        public async Task<IActionResult> RevokeRole(
            string userId,
            [FromBody] AssignRoleRequest request,
            CancellationToken ct)
        {
            var result = await adminService.RevokeRoleAsync(userId, request, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        // ╔══════════════════════════════════════════════════════════════════════
        // ║  PRODUCT MANAGEMENT
        // ╚══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// List all products (active and inactive) with seller info.
        /// Supports filtering by search, category, seller, and active status.
        /// </summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] AdminProductFilterDto filter, CancellationToken ct)
        {
            var result = await adminService.GetProductsAsync(filter, ct);
            return Ok(result.Value);
        }

        /// <summary>
        /// Activate or deactivate any product regardless of seller.
        /// Deactivated products are hidden from the public catalog.
        /// </summary>
        [HttpPatch("products/{productId:int}/active")]
        public async Task<IActionResult> SetProductActive(
            int productId,
            [FromQuery] bool isActive,
            CancellationToken ct)
        {
            var result = await adminService.SetProductActiveAsync(productId, isActive, ct);
            return result.IsSuccess
                ? Ok(new { message = isActive ? "Product activated." : "Product deactivated." })
                : result.ToProblem(StatusCodes.Status404NotFound);
        }
    }
}