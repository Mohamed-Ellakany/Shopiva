using Shopiva.Contracts.Payment;

namespace Shopiva.Interfaces
{
    public interface IPaymentService
    {
        Task<OrderWithPaymentResponse> CreateOrderWithPaymentAsync(string customerId, CreateOrderWithPaymentRequest request);
        Task<PaymentResponse> ProcessPaymentAsync(string customerId, ProcessPaymentRequest request);
        Task<PaymentResponse> RefundPaymentAsync(string customerId, RefundPaymentRequest request);
        Task<PaymentHistoryResponse> GetPaymentHistoryAsync(string customerId, int page = 1, int pageSize = 10);
        Task<PaymentResponse> GetPaymentByIdAsync(string customerId, int paymentId);
    }
}
