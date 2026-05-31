using Microsoft.EntityFrameworkCore;
using Shopiva.Abstractions.enums;
using Shopiva.Contracts.Payment;
using Shopiva.Data;
using Shopiva.Interfaces;
using Shopiva.Models;

namespace Shopiva.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────
        // 1. Create Order + Payment (COD)
        // ────────────────────────────────────────────
        public async Task<OrderWithPaymentResponse> CreateOrderWithPaymentAsync(
            string customerId, CreateOrderWithPaymentRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                throw new InvalidOperationException("Order must have at least one item.");

            // Validate products & stock
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            if (products.Count != productIds.Count)
                throw new InvalidOperationException("One or more products are invalid or inactive.");

            var orderItems = new List<OrderItem>();
            decimal subTotal = 0;

            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'.");

                var unitPrice = product.DiscountedPrice ?? product.Price;
                subTotal += unitPrice * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = unitPrice,
                    Quantity = item.Quantity
                });

                // Reduce stock
                product.Stock -= item.Quantity;
            }

            // Create Order
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customerId,
                ShippingAddress = request.ShippingAddress,
                ShippingCity = request.ShippingCity,
                ShippingCountry = request.ShippingCountry,
                SubTotal = subTotal,
                ShippingFee = 0,
                Notes = request.Notes,
                Status = OrderStatus.Pending,
                Items = orderItems
            };

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = OrderStatus.Pending,
                ChangedAt = DateTime.UtcNow,
                Note = "Order created (COD)"
            });

            // Create Payment
            var payment = new Payment
            {
                PaymentNumber = GeneratePaymentNumber(),
                Amount = order.Total,
                Method = PaymentMethod.COD,
                Status = PaymentStatus.Pending,
                Notes = "Cash on Delivery - awaiting collection"
            };

            order.Payment = payment;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return MapToOrderWithPaymentResponse(order, payment);
        }

        // ────────────────────────────────────────────
        // 2. Process Payment (Mark as Paid)
        // ────────────────────────────────────────────
        public async Task<PaymentResponse> ProcessPaymentAsync(
            string customerId, ProcessPaymentRequest request)
        {
            var payment = await GetPaymentWithOrderAsync(customerId, request.OrderId)
                ?? throw new KeyNotFoundException("Payment not found.");

            if (payment.Status != PaymentStatus.Pending)
                throw new InvalidOperationException($"Payment is already '{payment.Status}'. Cannot process again.");

            payment.Status = PaymentStatus.Paid;
            payment.Notes = request.Notes ?? "Payment collected (COD)";
            payment.UpdatedAt = DateTime.UtcNow;

            // Update order status
            payment.Order.Status = OrderStatus.Processing;
            payment.Order.UpdatedAt = DateTime.UtcNow;
            payment.Order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = OrderStatus.Processing,
                ChangedAt = DateTime.UtcNow,
                Note = "Payment received"
            });

            await _context.SaveChangesAsync();

            return MapToPaymentResponse(payment);
        }

        // ────────────────────────────────────────────
        // 3. Refund Payment
        // ────────────────────────────────────────────
        public async Task<PaymentResponse> RefundPaymentAsync(
            string customerId, RefundPaymentRequest request)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Items)
                .FirstOrDefaultAsync(p =>
                    p.Id == request.PaymentId &&
                    p.Order.CustomerId == customerId)
                ?? throw new KeyNotFoundException("Payment not found.");

            if (payment.Status != PaymentStatus.Paid)
                throw new InvalidOperationException("Only paid payments can be refunded.");

            payment.Status = PaymentStatus.Refunded;
            payment.Notes = request.Reason ?? "Refund processed";
            payment.UpdatedAt = DateTime.UtcNow;

            // Restore stock
            foreach (var item in payment.Order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.Stock += item.Quantity;
            }

            // Update order status
            payment.Order.Status = OrderStatus.Cancelled;
            payment.Order.UpdatedAt = DateTime.UtcNow;
            payment.Order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = OrderStatus.Cancelled,
                ChangedAt = DateTime.UtcNow,
                Note = $"Refunded: {request.Reason}"
            });

            await _context.SaveChangesAsync();

            return MapToPaymentResponse(payment);
        }

        // ────────────────────────────────────────────
        // 4. Payment History
        // ────────────────────────────────────────────
        public async Task<PaymentHistoryResponse> GetPaymentHistoryAsync(
            string customerId, int page = 1, int pageSize = 10)
        {
            var query = _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Order.CustomerId == customerId)
                .OrderByDescending(p => p.CreatedAt);

            var total = await query.CountAsync();

            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaymentHistoryResponse(
                total,
                payments.Select(MapToPaymentResponse).ToList()
            );
        }

        // ────────────────────────────────────────────
        // 5. Get Payment By Id
        // ────────────────────────────────────────────
        public async Task<PaymentResponse> GetPaymentByIdAsync(string customerId, int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p =>
                    p.Id == paymentId &&
                    p.Order.CustomerId == customerId)
                ?? throw new KeyNotFoundException("Payment not found.");

            return MapToPaymentResponse(payment);
        }

        // ────────────────────────────────────────────
        // Helpers
        // ────────────────────────────────────────────
        private async Task<Payment?> GetPaymentWithOrderAsync(string customerId, int orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.StatusHistory)
                .FirstOrDefaultAsync(p =>
                    p.OrderId == orderId &&
                    p.Order.CustomerId == customerId);
        }

        private static string GenerateOrderNumber()
            => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        private static string GeneratePaymentNumber()
            => $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        private static PaymentResponse MapToPaymentResponse(Payment p) => new(
            p.Id,
            p.PaymentNumber,
            p.OrderId,
            p.Order.OrderNumber,
            p.Amount,
            p.Method.ToString(),
            p.Status.ToString(),
            p.Notes,
            p.CreatedAt,
            p.UpdatedAt
        );

        private static OrderWithPaymentResponse MapToOrderWithPaymentResponse(Order o, Payment p) => new(
            o.Id,
            o.OrderNumber,
            o.Status.ToString(),
            o.SubTotal,
            o.ShippingFee,
            o.Total,
            MapToPaymentResponse(p),
            o.CreatedAt
        );
    }
}
