using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Shopiva.Contracts.Orders.OrderDtos;

namespace Shopiva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController(IOrderService orderService) : ControllerBase
    {
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        private bool IsAdmin => User.IsInRole("Admin");

        // ── Customer endpoints ────────────────────────────────────────────────

        /// <summary>Place a new order from the active cart.</summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PlaceOrder(
            [FromBody] PlaceOrderRequest request, CancellationToken ct)
        {
            var result = await orderService.PlaceOrderAsync(UserId, request, ct);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        /// <summary>Get a specific order (own order for Customer, any order for Admin).</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await orderService.GetByIdAsync(id, UserId, IsAdmin, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status404NotFound);
        }

        /// <summary>List the authenticated customer's own orders.</summary>
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await orderService.GetMyOrdersAsync(UserId, page, pageSize, ct);
            return Ok(result.Value);
        }

        /// <summary>Customer cancels own order (Pending or Confirmed only).</summary>
        [HttpPost("{id:int}/cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            var result = await orderService.CancelAsync(id, UserId, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }

        // ── Admin endpoints ───────────────────────────────────────────────────

        /// <summary>List all orders with optional status filter.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] OrderStatus? status = null,
            CancellationToken ct = default)
        {
            var result = await orderService.GetAllOrdersAsync(page, pageSize, status, ct);
            return Ok(result.Value);
        }

        /// <summary>Admin/Seller: update order status (e.g. Confirmed → Shipped).</summary>
        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
        {
            var result = await orderService.UpdateStatusAsync(id, request, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToProblem(StatusCodes.Status400BadRequest);
        }
    }
}