using System.Diagnostics;
using System.Net;
using AutoMapper;
using Cart.API.Entities;
using Cart.API.Repositories.Interfaces;
using EventBusMessages.Common;
using EventBusMessages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Cart.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CartController : ControllerBase
    {
        private static readonly ActivitySource Activity = new(nameof(CartController));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly ICartRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public CartController(ICartRepository repository, IPublishEndpoint publishEndpoint, IMapper mapper, ILogger<CartController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{username}", Name = "GetCart")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetCart(string username)
        {
            var basket = await _repository.GetCart(username);
            return Ok(basket ?? new ShoppingCart(username));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateCart([FromBody] ShoppingCart cart)
        {
            return Ok(await _repository.UpdateCart(cart));
        }

        [HttpDelete("{username}", Name = "DeleteCart")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteCart(string username)
        {
            await _repository.DeleteCart(username);
            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] CartCheckout cartCheckout)
        {
            using (var activity = Activity.StartActivity("Publish Cart Checkout Event", ActivityKind.Producer))
            {
                // Get existing basket with total price
                var cart = await _repository.GetCart(cartCheckout.Username);
                if (cart == null)
                {
                    return BadRequest();
                }

                // Send checkout event to rabbitmq
                var eventMessage = _mapper.Map<CartCheckoutEvent>(cartCheckout);
                eventMessage.TotalPrice = cart.TotalPrice;
                await _publishEndpoint.Publish(eventMessage, c => AddActivityToHeader(activity, c));

                // Remove the cart
                await _repository.DeleteCart(cart.Username);

                return Accepted();
            }
        }

        private void AddActivityToHeader(Activity activity, PublishContext context)
        {
            Propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), context, InjectContextIntoHeader);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.rabbitmq.queue", EventBusConstants.CartCheckoutQueue);
        }

        private void InjectContextIntoHeader(PublishContext context, string key, string value)
        {
            try
            {
                _logger.LogDebug($"Setting header {key}:{value} for cart checkout event tracing");
                context?.Headers.Set(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to inject trace context.");
            }
        }
    }
}