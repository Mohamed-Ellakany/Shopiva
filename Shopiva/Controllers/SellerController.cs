using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shopiva.Contracts.SellerDashboard;

namespace Shopiva.Controllers
{
    [ApiController]
    [Route("api/seller")]
    [Authorize(Roles = "Seller,Admin")]
    public class SellerController(ISellerService sellerService) : ControllerBase
    {
        private string SellerId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Seller dashboard — products stats, order stats, revenue, ratings.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
        {
            var result = await sellerService.GetDashboardAsync(SellerId, ct);
            return Ok(result.Value);
        }

        /// <summary>
        /// List only this seller's own products (including inactive ones).
        /// Supports search, category, active/stock filters and sorting.
        /// </summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetMyProducts(
            [FromQuery] SellerProductFilterDto filter, CancellationToken ct)
        {
            var result = await sellerService.GetMyProductsAsync(SellerId, filter, ct);
            return Ok(result.Value);
        }

        /// <summary>
        /// List orders that contain at least one of this seller's products.
        /// Totals show only this seller's share of each order.
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] SellerOrderFilterDto filter, CancellationToken ct)
        {
            var result = await sellerService.GetMyOrdersAsync(SellerId, filter, ct);
            return Ok(result.Value);
        }
    }
}
