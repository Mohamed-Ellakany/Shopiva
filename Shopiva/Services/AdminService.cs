namespace Shopiva.Services
{
    public class AdminService(
        UserManager<ApplicationUser> userManager,
        AppDbContext context) : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly AppDbContext _context = context;

        // ── Dashboard ──────────────────────────────────────────────────────────

        public async Task<Result<DashboardSummaryDto>> GetDashboardAsync(CancellationToken ct = default)
        {
            // Fetch user role memberships efficiently
            var customerRoleId = await _context.Roles
                .Where(r => r.NormalizedName == "CUSTOMER")
                .Select(r => r.Id)
                .FirstOrDefaultAsync(ct);

            var sellerRoleId = await _context.Roles
                .Where(r => r.NormalizedName == "SELLER")
                .Select(r => r.Id)
                .FirstOrDefaultAsync(ct);

            var totalUsers = await _userManager.Users.CountAsync(ct);

            var totalCustomers = customerRoleId is null ? 0 :
                await _context.UserRoles.CountAsync(ur => ur.RoleId == customerRoleId, ct);

            var totalSellers = sellerRoleId is null ? 0 :
                await _context.UserRoles.CountAsync(ur => ur.RoleId == sellerRoleId, ct);

            var restrictedUsers = await _userManager.Users
                .CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow, ct);

            var totalProducts = await _context.Products.CountAsync(ct);
            var activeProducts = await _context.Products.CountAsync(p => p.IsActive, ct);
            var outOfStockProducts = await _context.Products.CountAsync(p => p.IsActive && p.Stock == 0, ct);
            var totalCategories = await _context.Categories.CountAsync(ct);
            var totalReviews = await _context.Reviews.CountAsync(ct);

            // Orders (uses Orders DbSet added by the Order module)
            var totalOrders = await _context.Orders.CountAsync(ct);
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
            var shippedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipped, ct);
            var deliveredOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered, ct);

            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.SubTotal + o.ShippingFee, ct);

            return Result.Success(new DashboardSummaryDto
            {
                TotalUsers = totalUsers,
                TotalCustomers = totalCustomers,
                TotalSellers = totalSellers,
                RestrictedUsers = restrictedUsers,
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                OutOfStockProducts = outOfStockProducts,
                TotalCategories = totalCategories,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ShippedOrders = shippedOrders,
                DeliveredOrders = deliveredOrders,
                TotalRevenue = totalRevenue,
                TotalReviews = totalReviews
            });
        }

        // ── User Management ────────────────────────────────────────────────────

        public async Task<Result<PaginatedResult<AdminUserSummaryDto>>> GetUsersAsync(
            UserFilterDto filter, CancellationToken ct = default)
        {
            // Build the query on the users table
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(u =>
                    u.Email!.ToLower().Contains(search) ||
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search));
            }

            if (filter.IsRestricted.HasValue)
            {
                query = filter.IsRestricted.Value
                    ? query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
                    : query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
            }

            // Role filter — join through UserRoles
            if (!string.IsNullOrWhiteSpace(filter.Role))
            {
                var roleId = await _context.Roles
                    .Where(r => r.NormalizedName == filter.Role.ToUpper())
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync(ct);

                if (roleId is not null)
                {
                    var userIdsInRole = _context.UserRoles
                        .Where(ur => ur.RoleId == roleId)
                        .Select(ur => ur.UserId);
                    query = query.Where(u => userIdsInRole.Contains(u.Id));
                }
            }

            var total = await query.CountAsync(ct);
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            // Resolve roles in bulk
            var dtos = new List<AdminUserSummaryDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                dtos.Add(MapToSummary(user, roles));
            }

            return Result.Success(new PaginatedResult<AdminUserSummaryDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }

        public async Task<Result<AdminUserDetailDto>> GetUserByIdAsync(
            string userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<AdminUserDetailDto>(AdminErrors.UserNotFound);

            var roles = await _userManager.GetRolesAsync(user);
            var totalOrders = await _context.Orders.CountAsync(o => o.CustomerId == userId, ct);
            var totalReviews = await _context.Reviews.CountAsync(r => r.CustomerId == userId, ct);

            return Result.Success(new AdminUserDetailDto
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
                IsRestricted = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd?.UtcDateTime,
                Roles = roles.ToList(),
                TotalOrders = totalOrders,
                TotalReviews = totalReviews
            });
        }

        public async Task<Result<bool>> RestrictUserAsync(
            string userId, RestrictUserRequest request, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<bool>(AdminErrors.UserNotFound);

            // Prevent restricting another admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return Result.Failure<bool>(AdminErrors.CannotRestrictAdmin);

            if (request.Restrict)
            {
                // Enable lockout and set end date (permanent = 100 years if not specified)
                await _userManager.SetLockoutEnabledAsync(user, true);
                var lockoutEnd = request.Until.HasValue
                    ? new DateTimeOffset(request.Until.Value, TimeSpan.Zero)
                    : DateTimeOffset.UtcNow.AddYears(100);

                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            }
            else
            {
                // Lift restriction
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            return Result.Success(true);
        }

        public async Task<Result<bool>> DeleteUserAsync(
            string userId, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<bool>(AdminErrors.UserNotFound);

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return Result.Failure<bool>(AdminErrors.CannotDeleteAdmin);

            // Soft-delete: lock the account permanently and anonymise PII
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            user.Email = $"deleted_{user.Id}@shopiva.invalid";
            user.UserName = $"deleted_{user.Id}";
            user.NormalizedEmail = user.Email.ToUpper();
            user.NormalizedUserName = user.UserName.ToUpper();
            user.FirstName = "Deleted";
            user.LastName = "User";
            user.PhoneNumber = null;
            user.Address = null;
            user.City = null;
            user.Country = null;
            user.ProfileImageUrl = null;

            // Invalidate all refresh tokens
            foreach (var rt in user.RefreshTokens.Where(r => r.IsActive))
                rt.RevokedOn = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result.Failure<bool>(new Error(
                    "Admin.DeleteFailed",
                    string.Join("; ", result.Errors.Select(e => e.Description))));

            return Result.Success(true);
        }

        public async Task<Result<AdminUserSummaryDto>> AssignRoleAsync(
            string userId, AssignRoleRequest request, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<AdminUserSummaryDto>(AdminErrors.UserNotFound);

            if (await _userManager.IsInRoleAsync(user, request.Role))
                return Result.Failure<AdminUserSummaryDto>(AdminErrors.UserAlreadyInRole);

            var result = await _userManager.AddToRoleAsync(user, request.Role);
            if (!result.Succeeded)
                return Result.Failure<AdminUserSummaryDto>(new Error(
                    "Admin.RoleFailed",
                    string.Join("; ", result.Errors.Select(e => e.Description))));

            var roles = await _userManager.GetRolesAsync(user);
            return Result.Success(MapToSummary(user, roles));
        }

        public async Task<Result<AdminUserSummaryDto>> RevokeRoleAsync(
            string userId, AssignRoleRequest request, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result.Failure<AdminUserSummaryDto>(AdminErrors.UserNotFound);

            // Cannot revoke Admin role from the last admin
            if (request.Role == "Admin")
            {
                var adminRoleId = await _context.Roles
                    .Where(r => r.NormalizedName == "ADMIN")
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync(ct);

                var adminCount = adminRoleId is null ? 0 :
                    await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRoleId, ct);

                if (adminCount <= 1)
                    return Result.Failure<AdminUserSummaryDto>(AdminErrors.CannotRemoveLastAdmin);
            }

            if (!await _userManager.IsInRoleAsync(user, request.Role))
                return Result.Failure<AdminUserSummaryDto>(AdminErrors.UserNotInRole);

            var result = await _userManager.RemoveFromRoleAsync(user, request.Role);
            if (!result.Succeeded)
                return Result.Failure<AdminUserSummaryDto>(new Error(
                    "Admin.RoleFailed",
                    string.Join("; ", result.Errors.Select(e => e.Description))));

            var roles = await _userManager.GetRolesAsync(user);
            return Result.Success(MapToSummary(user, roles));
        }

        // ── Product Management ─────────────────────────────────────────────────

        public async Task<Result<PaginatedResult<AdminProductSummaryDto>>> GetProductsAsync(
            AdminProductFilterDto filter, CancellationToken ct = default)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.Reviews)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    p.Description.ToLower().Contains(s));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId);

            if (filter.IsActive.HasValue)
                query = query.Where(p => p.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.SellerId))
                query = query.Where(p => p.SellerId == filter.SellerId);

            query = filter.SortBy.ToLower() switch
            {
                "price" => filter.Descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "name" => filter.Descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "stock" => filter.Descending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
                _ => filter.Descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new AdminProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    IsActive = p.IsActive,
                    CategoryName = p.Category.Name,
                    SellerName = p.Seller.UserName ?? string.Empty,
                    SellerEmail = p.Seller.Email ?? string.Empty,
                    AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = p.Reviews.Count,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync(ct);

            return Result.Success(new PaginatedResult<AdminProductSummaryDto>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }

        public async Task<Result<bool>> SetProductActiveAsync(
            int productId, bool isActive, CancellationToken ct = default)
        {
            var product = await _context.Products.FindAsync([productId], ct);
            if (product is null)
                return Result.Failure<bool>(UserErrors.ProductNotFound);

            product.IsActive = isActive;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return Result.Success(true);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static AdminUserSummaryDto MapToSummary(
            ApplicationUser user, IList<string> roles) => new()
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsRestricted = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd?.UtcDateTime,
                Roles = roles.ToList()
            };
    }
}
