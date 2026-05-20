using Shopiva.Contracts.Cart;
using Shopiva.Interfaces.Cart;

namespace Shopiva.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;


        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }


        [HttpGet]
        public async Task<IActionResult> GetCart(CancellationToken ct)
        {
            var result = await _cartService.GetCartAsync(UserId, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : NotFound(result.Error);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto, CancellationToken ct)
        {
            var result = await _cartService.AddItemAsync(UserId, dto, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemDto dto, CancellationToken ct)
        {
            var result = await _cartService.UpdateItemAsync(UserId, dto, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }


        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken ct)
        {
            var result = await _cartService.RemoveItemAsync(UserId, cartItemId, ct);
            return result.IsSuccess
                ? NoContent()
                : NotFound(result.Error);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart(CancellationToken ct)
        {
            var result = await _cartService.ClearCartAsync(UserId, ct);
            return result.IsSuccess
                ? NoContent()
                : NotFound(result.Error);
        }
    }
}
