namespace Shopiva.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
       
            [HttpGet]
            public async Task<IActionResult> GetProductReviews(int productId)
            {
                var result = await reviewService.GetProductReviewsAsync(productId);
                return result.IsSuccess ? Ok(result.Value) : result.ToProblem(StatusCodes.Status404NotFound);
            }

            [HttpGet("{reviewId}")]
            public async Task<IActionResult> GetById(int productId, int reviewId)
            {
                var result = await reviewService.GetByIdAsync(reviewId);
                return result.IsSuccess ? Ok(result.Value) : result.ToProblem(StatusCodes.Status404NotFound);
            }

            [HttpPost]
            [Authorize(Roles = "Customer")]
            public async Task<IActionResult> Create(int productId, [FromBody] CreateReviewDto dto)
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await reviewService.CreateAsync(productId, dto, customerId);
                return result.IsSuccess
                    ? CreatedAtAction(nameof(GetById), new { productId, reviewId = result.Value.Id }, result.Value)
                    : result.ToProblem(StatusCodes.Status400BadRequest);
            }

            [HttpPut("{reviewId}")]
            [Authorize(Roles = "Customer")]
            public async Task<IActionResult> Update(int productId, int reviewId, [FromBody] UpdateReviewDto dto)
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await reviewService.UpdateAsync(reviewId, dto, customerId);
                return result.IsSuccess ? Ok(result.Value) : result.ToProblem(StatusCodes.Status400BadRequest);
            }

            [HttpDelete("{reviewId}")]
            [Authorize]
            public async Task<IActionResult> Delete(int productId, int reviewId)
            {
                var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var isAdmin = User.IsInRole("Admin");
                var result = await reviewService.DeleteAsync(reviewId, customerId, isAdmin);
                return result.IsSuccess ? NoContent() : result.ToProblem(StatusCodes.Status400BadRequest);
            }
        }
    }

