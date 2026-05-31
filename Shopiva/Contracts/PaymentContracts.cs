using Shopiva.Abstractions.enums;

namespace Shopiva.Contracts.Payment
{
    // ────────── Request DTOs ──────────

    public record CreateOrderWithPaymentRequest(
        string ShippingAddress,
        string ShippingCity,
        string ShippingCountry,
        string? Notes,
        List<OrderItemRequest> Items
    );

    public record OrderItemRequest(
        int ProductId,
        int Quantity
    );

    public record ProcessPaymentRequest(
        int OrderId,
        string? Notes
    );

    public record RefundPaymentRequest(
        int PaymentId,
        string? Reason
    );

    // ────────── Response DTOs ──────────

    public record PaymentResponse(
        int Id,
        string PaymentNumber,
        int OrderId,
        string OrderNumber,
        decimal Amount,
        string Method,
        string Status,
        string? Notes,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    public record OrderWithPaymentResponse(
        int OrderId,
        string OrderNumber,
        string Status,
        decimal SubTotal,
        decimal ShippingFee,
        decimal Total,
        PaymentResponse Payment,
        DateTime CreatedAt
    );

    public record PaymentHistoryResponse(
        int TotalCount,
        List<PaymentResponse> Payments
    );
}
