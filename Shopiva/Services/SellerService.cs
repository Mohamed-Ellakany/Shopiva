using Shopiva.Contracts.SellerDashboard;

namespace Shopiva.Services
{
    public class SellerService : ISellerService
    {
        private readonly AppDbContext _context;

        public SellerService(AppDbContext context)
        {
            _context = context;
        }

        // ── Dashboard ─────────────────────────────────────────────────────────

        public async Task<Result<SellerDashboardDto>> GetDashboardAsync(
            string sellerId, CancellationToken ct = default)
        {
            // ── Products ──────────────────────────────────────────────────────
            var productsQuery = _context.Products.Where(p => p.SellerId == sellerId);

            var totalProducts = await productsQuery.CountAsync(ct);
            var activeProducts = await productsQuery.CountAsync(p => p.IsActive, ct);
            var outOfStock = await productsQuery.CountAsync(p => p.IsActive && p.Stock == 0, ct);

            var productIds = await productsQuery.Select(p => p.Id).ToListAsync(ct);

            // ── Reviews ───────────────────────────────────────────────────────
            var reviews = await _context.Reviews
                .Where(r => productIds.Contains(r.ProductId))
                .ToListAsync(ct);

            var totalReviews = reviews.Count;
            var averageRating = totalReviews > 0
                ? Math.Round(reviews.Average(r => r.Rating), 2)
                : 0;

            // ── Orders ────────────────────────────────────────────────────────
            // Orders that contain at least one product from this seller
            var sellerOrderIds = await _context.OrderItems
                .Where(oi => productIds.Contains(oi.ProductId))
                .Select(oi => oi.OrderId)
                .Distinct()
                .ToListAsync(ct);

            var ordersQuery = _context.Orders.Where(o => sellerOrderIds.Contains(o.Id));

            var totalOrders = await ordersQuery.CountAsync(ct);
            var pendingOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Pending, ct);
            var processingOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Processing, ct);
            var shippedOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Shipped, ct);
            var deliveredOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Delivered, ct);

            // Revenue: sum only THIS seller's items inside delivered orders
            var deliveredOrderIds = await ordersQuery
                .Where(o => o.Status == OrderStatus.Delivered)
                .Select(o => o.Id)
                .ToListAsync(ct);

            var totalRevenue = await _context.OrderItems
                .Where(oi => productIds.Contains(oi.ProductId)
                          && deliveredOrderIds.Contains(oi.OrderId))
                .SumAsync(oi => oi.UnitPrice * oi.Quantity, ct);

            return Result.Success(new SellerDashboardDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                OutOfStockProducts = outOfStock,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ProcessingOrders = processingOrders,
                ShippedOrders = shippedOrders,
                DeliveredOrders = deliveredOrders,
                TotalRevenue = totalRevenue,
                AverageRating = averageRating,
                TotalReviews = totalReviews
            });
        }

        // ── My Products ───────────────────────────────────────────────────────

        public async Task<Result<PaginatedResult<SellerProductSummaryDto>>> GetMyProductsAsync(
            string sellerId, SellerProductFilterDto filter, CancellationToken ct = default)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.SellerId == sellerId)
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

            if (filter.InStock == true)
                query = query.Where(p => p.Stock > 0);

            query = filter.SortBy.ToLower() switch
            {
                "price" => filter.Descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "name" => filter.Descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "stock" => filter.Descending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
                _ => filter.Descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };

            var total = await query.CountAsync(ct);

            var pagedProducts = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            // Units sold per product from delivered order items
            var pagedIds = pagedProducts.Select(p => p.Id).ToList();

            var unitsSold = await _context.OrderItems
                .Where(oi => pagedIds.Contains(oi.ProductId))
                .Join(_context.Orders.Where(o => o.Status == OrderStatus.Delivered),
                      oi => oi.OrderId,
                      o => o.Id,
                      (oi, _) => new { oi.ProductId, oi.Quantity })
                .GroupBy(x => x.ProductId)
                .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.Total, ct);

            var dtos = pagedProducts.Select(p => new SellerProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                DiscountedPrice = p.DiscountedPrice,
                Stock = p.Stock,
                IsActive = p.IsActive,
                CategoryName = p.Category?.Name ?? string.Empty,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = p.Reviews.Count,
                TotalUnitsSold = unitsSold.GetValueOrDefault(p.Id, 0),
                CreatedAt = p.CreatedAt,
                ImageUrls = p.Images.Select(i => i.Url).ToList()
            }).ToList();

            return Result.Success(new PaginatedResult<SellerProductSummaryDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }

        // ── My Orders ─────────────────────────────────────────────────────────

        public async Task<Result<PaginatedResult<SellerOrderSummaryDto>>> GetMyOrdersAsync(
            string sellerId, SellerOrderFilterDto filter, CancellationToken ct = default)
        {
            var productIds = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Select(p => p.Id)
                .ToListAsync(ct);

            var sellerOrderIds = await _context.OrderItems
                .Where(oi => productIds.Contains(oi.ProductId))
                .Select(oi => oi.OrderId)
                .Distinct()
                .ToListAsync(ct);

            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .Where(o => sellerOrderIds.Contains(o.Id))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<OrderStatus>(filter.Status, true, out var parsedStatus))
                query = query.Where(o => o.Status == parsedStatus);

            query = query.OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync(ct);
            var orders = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            var dtos = orders.Select(o =>
            {
                var sellerItems = o.Items.Where(i => productIds.Contains(i.ProductId)).ToList();
                return new SellerOrderSummaryDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerEmail = o.Customer?.Email ?? string.Empty,
                    Status = o.Status.ToString(),
                    SellerItemsTotal = sellerItems.Sum(i => i.UnitPrice * i.Quantity),
                    SellerItemCount = sellerItems.Count,
                    CreatedAt = o.CreatedAt
                };
            }).ToList();

            return Result.Success(new PaginatedResult<SellerOrderSummaryDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }
    }
}