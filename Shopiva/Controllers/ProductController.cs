namespace Shopiva.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter)
        {
            var result = await _productService.GetAllAsync(filter);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }


        [HttpPost]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _productService.CreateAsync(dto, sellerId);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
                : BadRequest(result.Error);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _productService.UpdateAsync(id, dto, sellerId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var isAdmin = User.IsInRole("Admin");
            var result = await _productService.DeleteAsync(id, sellerId, isAdmin);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }
    }
}

