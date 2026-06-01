using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace Shopiva.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentController(IConfiguration config)
        {
            _config = config;
            StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        }

        // POST api/payment/create-intent
        [HttpPost("create-intent")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100),
                    Currency = "usd",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "orderId",    request.OrderId.ToString() },
                        { "customerId", User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "" }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                return Ok(new
                {
                    clientSecret = intent.ClientSecret,
                    paymentIntentId = intent.Id,
                    publishableKey = _config["Stripe:PublishableKey"]
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST api/payment/webhook
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    Console.WriteLine($"Payment succeeded: {paymentIntent?.Id}");
                }
                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET api/payment/config
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            return Ok(new { publishableKey = _config["Stripe:PublishableKey"] });
        }
    }

    public record CreatePaymentIntentRequest(int OrderId, decimal Amount);
}