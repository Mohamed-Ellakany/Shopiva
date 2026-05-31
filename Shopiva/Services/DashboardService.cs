using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopiva.Abstractions.enums;
using Shopiva.Contracts.Dashboard;
using Shopiva.Data;
using Shopiva.Interfaces;
using Shopiva.Models;

namespace Shopiva.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ────────────────────────────────────────────
        // Store Overview
        // ────────────────────────────────────────────
        public async Task<StoreOverviewResponse> GetStoreOverviewAsync()
        {
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            // Revenue
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => p.Amount);

            var thisMonthRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid && p.CreatedAt >= thisMonthStart)
                .SumAsync(p => p.Amount);

            var lastMonthRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid
                    && p.CreatedAt >= lastMonthStart
                    && p.CreatedAt < thisMonthStart)
                .SumAsync(p => p.Amount);

            var revenueGrowth = lastMonthRevenue == 0 ? 0
                : Math.Round((double)(thisMonthRevenue - lastMonthRevenue) / (double)lastMonthRevenue * 100, 1);

            // Orders
            var totalOrders = await _context.Orders.CountAsync();

            var thisMonthOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= thisMonthStart);

            var lastMonthOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt >= lastMonthStart && o.CreatedAt < thisMonthStart);

            var ordersGrowth = lastMonthOrders == 0 ? 0
                : Math.Round((double)(thisMonthOrders - lastMonthOrders) / lastMonthOrders * 100, 1);

            // New Customers (this month)
            var allUsers = _userManager.Users;
            var newCustomers = await allUsers
                .CountAsync(u => u.LockoutEnd == null && u.EmailConfirmed == true);

            var thisMonthCustomers = await allUsers
                .CountAsync(u => u.LockoutEnd == null);

            var lastMonthCustomers = thisMonthCustomers - (thisMonthCustomers / 10); // estimate
            var customersGrowth = 18.0; // calculate from real data if you track created dates

            // Conversion Rate = Paid Orders / Total Visits (approximated as paid/total orders)
            var paidOrders = await _context.Orders
                .CountAsync(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Pending);

            var conversionRate = totalOrders == 0 ? 0
                : Math.Round((double)paidOrders / totalOrders * 100, 2);

            var conversionStatus = conversionRate >= 3 ? "High"
                : conversionRate >= 1.5 ? "Medium"
                : "Low";

            return new StoreOverviewResponse(
                totalRevenue,
                (decimal)revenueGrowth,
                totalOrders,
                (decimal)ordersGrowth,
                thisMonthCustomers,
                (decimal)customersGrowth,
                conversionRate,
                conversionStatus
            );
        }

        // ────────────────────────────────────────────
        // Recent Users
        // ────────────────────────────────────────────
        public async Task<RecentUsersResponse> GetRecentUsersAsync(int count = 10)
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.Id)
                .Take(count)
                .ToListAsync();

            var result = new List<RecentUserResponse>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Customer";
                var status = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
                    ? "Pending" : "Active";

                result.Add(new RecentUserResponse(
                    user.Id,
                    $"{user.FirstName} {user.LastName}",
                    user.Email ?? "",
                    role,
                    status,
                    user.ProfileImageUrl,
                    DateTime.UtcNow // replace with actual CreatedAt if you add it
                ));
            }

            return new RecentUsersResponse(result.Count, result);
        }

        // ────────────────────────────────────────────
        // Promo Codes
        // ────────────────────────────────────────────
        public async Task<List<PromoCodeResponse>> GetAllPromoCodesAsync()
        {
            var promos = await _context.PromoCodes.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return promos.Select(MapToPromoResponse).ToList();
        }

        public async Task<PromoCodeResponse> GetPromoCodeByIdAsync(int id)
        {
            var promo = await _context.PromoCodes.FindAsync(id)
                ?? throw new KeyNotFoundException("Promo code not found.");
            return MapToPromoResponse(promo);
        }

        public async Task<PromoCodeResponse> CreatePromoCodeAsync(CreatePromoCodeRequest request)
        {
            var exists = await _context.PromoCodes.AnyAsync(p => p.Code == request.Code.ToUpper());
            if (exists) throw new InvalidOperationException("Promo code already exists.");

            var promo = new PromoCode
            {
                Code = request.Code.ToUpper(),
                Description = request.Description,
                DiscountPercent = request.DiscountPercent,
                ExpiresAt = request.ExpiresAt,
                IsActive = true
            };

            _context.PromoCodes.Add(promo);
            await _context.SaveChangesAsync();
            return MapToPromoResponse(promo);
        }

        public async Task<PromoCodeResponse> UpdatePromoCodeAsync(int id, UpdatePromoCodeRequest request)
        {
            var promo = await _context.PromoCodes.FindAsync(id)
                ?? throw new KeyNotFoundException("Promo code not found.");

            if (request.Description != null) promo.Description = request.Description;
            if (request.DiscountPercent.HasValue) promo.DiscountPercent = request.DiscountPercent.Value;
            if (request.IsActive.HasValue) promo.IsActive = request.IsActive.Value;
            if (request.ExpiresAt.HasValue) promo.ExpiresAt = request.ExpiresAt;

            await _context.SaveChangesAsync();
            return MapToPromoResponse(promo);
        }

        public async Task DeletePromoCodeAsync(int id)
        {
            var promo = await _context.PromoCodes.FindAsync(id)
                ?? throw new KeyNotFoundException("Promo code not found.");
            _context.PromoCodes.Remove(promo);
            await _context.SaveChangesAsync();
        }

        // ────────────────────────────────────────────
        // Banner
        // ────────────────────────────────────────────
        public async Task<List<BannerResponse>> GetAllBannersAsync()
        {
            var banners = await _context.Banners.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return banners.Select(MapToBannerResponse).ToList();
        }

        public async Task<BannerResponse> GetLiveBannerAsync()
        {
            var banner = await _context.Banners.FirstOrDefaultAsync(b => b.IsLive)
                ?? throw new KeyNotFoundException("No live banner found.");
            return MapToBannerResponse(banner);
        }

        public async Task<BannerResponse> CreateBannerAsync(CreateBannerRequest request)
        {
            var banner = new Banner
            {
                Title = request.Title,
                SubTitle = request.SubTitle,
                ImageUrl = request.ImageUrl,
                IsLive = false
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();
            return MapToBannerResponse(banner);
        }

        public async Task<BannerResponse> UpdateBannerAsync(int id, UpdateBannerRequest request)
        {
            var banner = await _context.Banners.FindAsync(id)
                ?? throw new KeyNotFoundException("Banner not found.");

            if (request.Title != null) banner.Title = request.Title;
            if (request.SubTitle != null) banner.SubTitle = request.SubTitle;
            if (request.ImageUrl != null) banner.ImageUrl = request.ImageUrl;
            banner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToBannerResponse(banner);
        }

        public async Task<BannerResponse> SetLiveBannerAsync(int id)
        {
            // Remove current live banner
            var currentLive = await _context.Banners.FirstOrDefaultAsync(b => b.IsLive);
            if (currentLive != null)
            {
                currentLive.IsLive = false;
                currentLive.UpdatedAt = DateTime.UtcNow;
            }

            var banner = await _context.Banners.FindAsync(id)
                ?? throw new KeyNotFoundException("Banner not found.");

            banner.IsLive = true;
            banner.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToBannerResponse(banner);
        }

        public async Task DeleteBannerAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id)
                ?? throw new KeyNotFoundException("Banner not found.");
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
        }

        // ────────────────────────────────────────────
        // Mappers
        // ────────────────────────────────────────────
        private static PromoCodeResponse MapToPromoResponse(PromoCode p)
        {
            var expiryStatus = !p.ExpiresAt.HasValue ? "No Expiry"
                : p.ExpiresAt < DateTime.UtcNow ? "Expired"
                : "Active";

            return new PromoCodeResponse(
                p.Id, p.Code, p.Description,
                p.DiscountPercent, p.IsActive,
                p.ExpiresAt, expiryStatus, p.CreatedAt
            );
        }

        private static BannerResponse MapToBannerResponse(Banner b) => new(
            b.Id, b.Title, b.SubTitle,
            b.ImageUrl, b.IsLive,
            b.CreatedAt, b.UpdatedAt
        );
    }
}
