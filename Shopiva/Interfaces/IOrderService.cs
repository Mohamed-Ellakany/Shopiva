using Shopiva.Abstractions.enums;
using static Shopiva.Contracts.Orders.OrderDtos;

namespace Shopiva.Interfaces
{
    public interface IOrderService
    {/// <summary>Customer: place order from active cart.</summary>
        Task<Result<OrderResponseDto>> PlaceOrderAsync(
            string customerId,
            PlaceOrderRequest request,
            CancellationToken ct = default);

        /// <summary>Customer: get own order by id.</summary>
        Task<Result<OrderResponseDto>> GetByIdAsync(
            int orderId,
            string customerId,
            bool isAdmin,
            CancellationToken ct = default);

        /// <summary>Customer: list own orders (paginated).</summary>
        Task<Result<PaginatedResult<OrderSummaryDto>>> GetMyOrdersAsync(
            string customerId,
            int page,
            int pageSize,
            CancellationToken ct = default);

        /// <summary>Admin: list all orders (paginated, optional status filter).</summary>
        Task<Result<PaginatedResult<OrderSummaryDto>>> GetAllOrdersAsync(
            int page,
            int pageSize,
            OrderStatus? statusFilter,
            CancellationToken ct = default);

        /// <summary>Admin/Seller: update order status with optional note.</summary>
        Task<Result<OrderResponseDto>> UpdateStatusAsync(
            int orderId,
            UpdateOrderStatusRequest request,
            CancellationToken ct = default);

        /// <summary>Customer: cancel own pending order.</summary>
        Task<Result<OrderResponseDto>> CancelAsync(
            int orderId,
            string customerId,
            CancellationToken ct = default);
    }
}