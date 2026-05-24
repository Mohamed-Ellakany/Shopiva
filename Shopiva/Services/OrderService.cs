
using static Shopiva.Contracts.Orders.OrderDtos;

namespace Shopiva.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderService(
            AppDbContext context,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _userManager = userManager;
        }

        // ── Place Order ───────────────────────────────────────────────────────

        public async Task<Result<OrderResponseDto>> PlaceOrderAsync(
            string customerId,
            PlaceOrderRequest request,
            CancellationToken ct = default)
        {
            // 1. Load active cart with items
            var cart = await _unitOfWork.Carts
                .GetActiveCartWithItemsTrackedAsync(customerId, ct);

            if (cart is null || !cart.Items.Any())
                return Result.Failure<OrderResponseDto>(OrderErrors.EmptyCart);

            // 2. Validate stock for every item
            var productIds = cart.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToListAsync(ct);

            foreach (var item in cart.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is null)
                    return Result.Failure<OrderResponseDto>(UserErrors.ProductNotFound);
                if (product.Stock < item.Quantity)
                    return Result.Failure<OrderResponseDto>(UserErrors.InsufficientStock);
            }

            // 3. Build order
            var orderNumber = GenerateOrderNumber();
            var subTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = customerId,
                Status = OrderStatus.Pending,
                ShippingAddress = request.ShippingAddress,
                ShippingCity = request.ShippingCity,
                ShippingCountry = request.ShippingCountry,
                SubTotal = subTotal,
                ShippingFee = 0,          // extend later for shipping providers
                Notes = request.Notes,
                Items = cart.Items.Select(i =>
                {
                    var p = products.First(pr => pr.Id == i.ProductId);
                    return new OrderItem
                    {
                        ProductId = i.ProductId,
                        ProductName = p.Name,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity
                    };
                }).ToList(),
                StatusHistory =
                [
                    new OrderStatusHistory
                    {
                        Status = OrderStatus.Pending,
                        Note = "Order placed by customer."
                    }
                ]
            };

            _context.Orders.Add(order);

            // 4. Decrement stock
            foreach (var item in cart.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.Stock -= item.Quantity;
            }

            // 5. Mark cart as checked-out
            cart.Status = CartStatus.CheckedOut;
            _unitOfWork.Carts.Update(cart);

            await _context.SaveChangesAsync(ct);

            // 6. Send confirmation email (fire-and-forget style — don't fail the request)
            var customer = await _userManager.FindByIdAsync(customerId);
            if (customer is not null)
            {
                _ = _emailService.SendAsync(
                    customer.Email!,
                    $"Order Confirmed – {orderNumber}",
                    BuildOrderConfirmationEmail(customer, order),
                    ct);
            }

            return Result.Success(MapToDto(order, customer));
        }

        // ── Get By Id ─────────────────────────────────────────────────────────

        public async Task<Result<OrderResponseDto>> GetByIdAsync(
            int orderId, string customerId, bool isAdmin, CancellationToken ct = default)
        {
            var order = await LoadOrderAsync(orderId, ct);
            if (order is null)
                return Result.Failure<OrderResponseDto>(OrderErrors.OrderNotFound);

            if (!isAdmin && order.CustomerId != customerId)
                return Result.Failure<OrderResponseDto>(UserErrors.UnauthorizedAccess);

            return Result.Success(MapToDto(order, order.Customer));
        }

        // ── Customer: my orders ───────────────────────────────────────────────

        public async Task<Result<PaginatedResult<OrderSummaryDto>>> GetMyOrdersAsync(
            string customerId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt);

            return Result.Success(await PaginateAsync(query, page, pageSize, ct));
        }

        // ── Admin: all orders ─────────────────────────────────────────────────

        public async Task<Result<PaginatedResult<OrderSummaryDto>>> GetAllOrdersAsync(
            int page, int pageSize, OrderStatus? statusFilter, CancellationToken ct = default)
        {
            var query = _context.Orders.AsQueryable();
            if (statusFilter.HasValue)
                query = query.Where(o => o.Status == statusFilter.Value);
            query = query.OrderByDescending(o => o.CreatedAt);

            return Result.Success(await PaginateAsync(query, page, pageSize, ct));
        }

        // ── Update Status (Admin / Seller) ────────────────────────────────────

        public async Task<Result<OrderResponseDto>> UpdateStatusAsync(
            int orderId, UpdateOrderStatusRequest request, CancellationToken ct = default)
        {
            var order = await LoadOrderAsync(orderId, ct);
            if (order is null)
                return Result.Failure<OrderResponseDto>(OrderErrors.OrderNotFound);

            // Guard: can't go back to Pending, can't update already-refunded order
            if (request.NewStatus == OrderStatus.Pending)
                return Result.Failure<OrderResponseDto>(OrderErrors.InvalidStatusTransition);
            if (order.Status == OrderStatus.Refunded)
                return Result.Failure<OrderResponseDto>(OrderErrors.InvalidStatusTransition);

            order.Status = request.NewStatus;
            order.UpdatedAt = DateTime.UtcNow;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = request.NewStatus,
                Note = request.Note
            });

            await _context.SaveChangesAsync(ct);

            // Notify customer
            if (order.Customer?.Email is not null)
            {
                _ = _emailService.SendAsync(
                    order.Customer.Email,
                    $"Order Update – {order.OrderNumber}",
                    BuildStatusUpdateEmail(order.Customer, order),
                    ct);
            }

            return Result.Success(MapToDto(order, order.Customer));
        }

        // ── Customer: cancel ──────────────────────────────────────────────────

        public async Task<Result<OrderResponseDto>> CancelAsync(
            int orderId, string customerId, CancellationToken ct = default)
        {
            var order = await LoadOrderAsync(orderId, ct);
            if (order is null)
                return Result.Failure<OrderResponseDto>(OrderErrors.OrderNotFound);

            if (order.CustomerId != customerId)
                return Result.Failure<OrderResponseDto>(UserErrors.UnauthorizedAccess);

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                return Result.Failure<OrderResponseDto>(OrderErrors.CannotCancelOrder);

            // Restore stock
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(ct);

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is not null)
                    product.Stock += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = OrderStatus.Cancelled,
                Note = "Cancelled by customer."
            });

            await _context.SaveChangesAsync(ct);

            return Result.Success(MapToDto(order, order.Customer));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task<Order?> LoadOrderAsync(int orderId, CancellationToken ct)
            => await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        private static string GenerateOrderNumber()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var suffix = Random.Shared.Next(1000, 9999);
            return $"ORD-{date}-{suffix}";
        }

        private static async Task<PaginatedResult<OrderSummaryDto>> PaginateAsync(
            IQueryable<Order> query, int page, int pageSize, CancellationToken ct)
        {
            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    Total = o.SubTotal + o.ShippingFee,
                    ItemCount = o.Items.Count,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync(ct);

            return new PaginatedResult<OrderSummaryDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private static OrderResponseDto MapToDto(Order o, ApplicationUser? customer) => new()
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerId = o.CustomerId,
            CustomerEmail = customer?.Email ?? string.Empty,
            Status = o.Status,
            ShippingAddress = o.ShippingAddress,
            ShippingCity = o.ShippingCity,
            ShippingCountry = o.ShippingCountry,
            SubTotal = o.SubTotal,
            ShippingFee = o.ShippingFee,
            Total = o.SubTotal + o.ShippingFee,
            Notes = o.Notes,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.Subtotal
            }).ToList(),
            StatusHistory = o.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDto
                {
                    Status = h.Status,
                    Note = h.Note,
                    ChangedAt = h.ChangedAt
                }).ToList()
        };

        // ── Email Templates ───────────────────────────────────────────────────

        private static string BuildOrderConfirmationEmail(ApplicationUser user, Order order)
        {
            var itemRows = string.Concat(order.Items.Select(i =>
                $"<tr><td style='padding:6px 0;border-bottom:1px solid #E5E7EB'>{i.ProductName}</td>" +
                $"<td style='padding:6px 0;border-bottom:1px solid #E5E7EB;text-align:center'>{i.Quantity}</td>" +
                $"<td style='padding:6px 0;border-bottom:1px solid #E5E7EB;text-align:right'>${i.UnitPrice:F2}</td>" +
                $"<td style='padding:6px 0;border-bottom:1px solid #E5E7EB;text-align:right'>${i.Subtotal:F2}</td></tr>"));

            return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head><meta charset="UTF-8"/><title>Order Confirmed</title></head>
                <body style="font-family:'Segoe UI',Arial,sans-serif;background:#F4F4F7;margin:0;padding:0;">
                  <div style="max-width:560px;margin:40px auto;background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,.1);">
                    <div style="background:#4F46E5;padding:28px 40px;text-align:center;">
                      <h1 style="color:#fff;margin:0;font-size:22px;">🛍️ Shopiva</h1>
                    </div>
                    <div style="padding:36px 40px;color:#374151;">
                      <h2 style="margin:0 0 8px;font-size:20px;">Order Confirmed!</h2>
                      <p style="margin:0 0 4px;">Hi <strong>{user.FirstName}</strong>,</p>
                      <p style="margin:0 0 24px;color:#6B7280;">Your order <strong>{order.OrderNumber}</strong> has been placed successfully.</p>

                      <table style="width:100%;border-collapse:collapse;font-size:14px;">
                        <thead>
                          <tr style="border-bottom:2px solid #E5E7EB;">
                            <th style="text-align:left;padding-bottom:8px">Product</th>
                            <th style="text-align:center;padding-bottom:8px">Qty</th>
                            <th style="text-align:right;padding-bottom:8px">Price</th>
                            <th style="text-align:right;padding-bottom:8px">Subtotal</th>
                          </tr>
                        </thead>
                        <tbody>{itemRows}</tbody>
                        <tfoot>
                          <tr><td colspan="3" style="padding-top:12px;text-align:right;font-weight:600">Total:</td>
                              <td style="padding-top:12px;text-align:right;font-weight:600">${(order.SubTotal + order.ShippingFee):F2}</td></tr>
                        </tfoot>
                      </table>

                      <hr style="border:none;border-top:1px solid #E5E7EB;margin:24px 0;"/>
                      <p style="margin:0;font-size:13px;color:#6B7280;">
                        <strong>Ship to:</strong> {order.ShippingAddress}, {order.ShippingCity}, {order.ShippingCountry}
                      </p>
                    </div>
                    <div style="background:#F9FAFB;padding:18px 40px;text-align:center;font-size:12px;color:#9CA3AF;">
                      &copy; {DateTime.UtcNow.Year} Shopiva. All rights reserved.
                    </div>
                  </div>
                </body>
                </html>
                """;
        }

        private static string BuildStatusUpdateEmail(ApplicationUser user, Order order)
        {
            var statusMessages = new Dictionary<OrderStatus, string>
            {
                [OrderStatus.Confirmed] = "Great news! Your order has been confirmed and is being prepared.",
                [OrderStatus.Processing] = "Your order is currently being packed and prepared for shipment.",
                [OrderStatus.Shipped] = "Your order is on the way! It has been handed to the carrier.",
                [OrderStatus.Delivered] = "Your order has been delivered. We hope you enjoy your purchase!",
                [OrderStatus.Cancelled] = "Your order has been cancelled.",
                [OrderStatus.Refunded] = "Your order has been refunded. Please allow a few days for the amount to appear."
            };

            var message = statusMessages.GetValueOrDefault(order.Status, "Your order status has been updated.");

            return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head><meta charset="UTF-8"/><title>Order Update</title></head>
                <body style="font-family:'Segoe UI',Arial,sans-serif;background:#F4F4F7;margin:0;padding:0;">
                  <div style="max-width:520px;margin:40px auto;background:#fff;border-radius:10px;overflow:hidden;box-shadow:0 2px 10px rgba(0,0,0,.1);">
                    <div style="background:#4F46E5;padding:28px 40px;text-align:center;">
                      <h1 style="color:#fff;margin:0;font-size:22px;">🛍️ Shopiva</h1>
                    </div>
                    <div style="padding:36px 40px;color:#374151;">
                      <h2 style="margin:0 0 8px;font-size:20px;">Order Update</h2>
                      <p style="margin:0 0 8px;">Hi <strong>{user.FirstName}</strong>,</p>
                      <p style="margin:0 0 4px;color:#6B7280;">Order <strong>{order.OrderNumber}</strong> status: <strong>{order.Status}</strong></p>
                      <p style="margin:0 0 0;color:#6B7280;line-height:1.7;">{message}</p>
                    </div>
                    <div style="background:#F9FAFB;padding:18px 40px;text-align:center;font-size:12px;color:#9CA3AF;">
                      &copy; {DateTime.UtcNow.Year} Shopiva. All rights reserved.
                    </div>
                  </div>
                </body>
                </html>
                """;
        }
    }
}