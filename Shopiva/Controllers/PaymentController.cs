using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Shopiva.Contracts.Payment;
using Shopiva.Interfaces;

namespace Shopiva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST api/payment/order
        // Create order + COD payment
        [HttpPost("order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderWithPaymentRequest request)
        {
            try
            {
                var customerId = GetCustomerId();
                var result = await _paymentService.CreateOrderWithPaymentAsync(customerId, request);
                return CreatedAtAction(nameof(GetPaymentById), new { id = result.Payment.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/payment/process
        // Mark payment as paid (delivery collected cash)
        [HttpPut("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var customerId = GetCustomerId();
                var result = await _paymentService.ProcessPaymentAsync(customerId, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/payment/refund
        // Refund a paid order
        [HttpPut("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentRequest request)
        {
            try
            {
                var customerId = GetCustomerId();
                var result = await _paymentService.RefundPaymentAsync(customerId, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/payment/history?page=1&pageSize=10
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var customerId = GetCustomerId();
            var result = await _paymentService.GetPaymentHistoryAsync(customerId, page, pageSize);
            return Ok(result);
        }

        // GET api/payment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            try
            {
                var customerId = GetCustomerId();
                var result = await _paymentService.GetPaymentByIdAsync(customerId, id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────
        private string GetCustomerId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(id))
                throw new UnauthorizedAccessException("User not authenticated.");
            return id;
        }
    }
}
