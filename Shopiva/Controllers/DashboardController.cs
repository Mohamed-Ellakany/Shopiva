using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopiva.Contracts.Dashboard;
using Shopiva.Interfaces;

namespace Shopiva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // ────────── Store Overview ──────────

        // GET api/dashboard/overview
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var result = await _dashboardService.GetStoreOverviewAsync();
            return Ok(result);
        }

        // ────────── Recent Users ──────────

        // GET api/dashboard/recent-users?count=10
        [HttpGet("recent-users")]
        public async Task<IActionResult> GetRecentUsers([FromQuery] int count = 10)
        {
            var result = await _dashboardService.GetRecentUsersAsync(count);
            return Ok(result);
        }

        // ────────── Promo Codes ──────────

        // GET api/dashboard/promo-codes
        [HttpGet("promo-codes")]
        public async Task<IActionResult> GetPromoCodes()
        {
            var result = await _dashboardService.GetAllPromoCodesAsync();
            return Ok(result);
        }

        // GET api/dashboard/promo-codes/{id}
        [HttpGet("promo-codes/{id}")]
        public async Task<IActionResult> GetPromoCode(int id)
        {
            try
            {
                var result = await _dashboardService.GetPromoCodeByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST api/dashboard/promo-codes
        [HttpPost("promo-codes")]
        public async Task<IActionResult> CreatePromoCode([FromBody] CreatePromoCodeRequest request)
        {
            try
            {
                var result = await _dashboardService.CreatePromoCodeAsync(request);
                return CreatedAtAction(nameof(GetPromoCode), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT api/dashboard/promo-codes/{id}
        [HttpPut("promo-codes/{id}")]
        public async Task<IActionResult> UpdatePromoCode(int id, [FromBody] UpdatePromoCodeRequest request)
        {
            try
            {
                var result = await _dashboardService.UpdatePromoCodeAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE api/dashboard/promo-codes/{id}
        [HttpDelete("promo-codes/{id}")]
        public async Task<IActionResult> DeletePromoCode(int id)
        {
            try
            {
                await _dashboardService.DeletePromoCodeAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ────────── Banner ──────────

        // GET api/dashboard/banners
        [HttpGet("banners")]
        public async Task<IActionResult> GetBanners()
        {
            var result = await _dashboardService.GetAllBannersAsync();
            return Ok(result);
        }

        // GET api/dashboard/banners/live
        [HttpGet("banners/live")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLiveBanner()
        {
            try
            {
                var result = await _dashboardService.GetLiveBannerAsync();
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST api/dashboard/banners
        [HttpPost("banners")]
        public async Task<IActionResult> CreateBanner([FromBody] CreateBannerRequest request)
        {
            var result = await _dashboardService.CreateBannerAsync(request);
            return Ok(result);
        }

        // PUT api/dashboard/banners/{id}
        [HttpPut("banners/{id}")]
        public async Task<IActionResult> UpdateBanner(int id, [FromBody] UpdateBannerRequest request)
        {
            try
            {
                var result = await _dashboardService.UpdateBannerAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT api/dashboard/banners/{id}/set-live
        [HttpPut("banners/{id}/set-live")]
        public async Task<IActionResult> SetLiveBanner(int id)
        {
            try
            {
                var result = await _dashboardService.SetLiveBannerAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE api/dashboard/banners/{id}
        [HttpDelete("banners/{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            try
            {
                await _dashboardService.DeleteBannerAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
