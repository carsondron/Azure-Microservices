using System.Diagnostics;
using AutoMapper;
using EventBusMessages.Common;
using EventBusMessages.Events;
using MassTransit;
using MediatR;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Orders.Application.Features.Orders.Commands.CheckoutOrder;

namespace Orders.API.EventBusConsumer
{
    public class CartCheckoutConsumer : IConsumer<CartCheckoutEvent>
    {
        private static readonly ActivitySource Activity = new(nameof(CartCheckoutConsumer));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<CartCheckoutConsumer> _logger;

        public CartCheckoutConsumer(IMediator mediator, IMapper mapper, ILogger<CartCheckoutConsumer> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<CartCheckoutEvent> context)
        {
            var parentContext = Propagator.Extract(default, context, ExtractTraceContextFromConsumeContext);
            Baggage.Current = parentContext.Baggage;

            using (var activity = Activity.StartActivity("Process Cart Checkout Event", ActivityKind.Consumer, parentContext.ActivityContext))
            {
                AddActivityTags(activity);
                var command = _mapper.Map<CheckoutOrderCommand>(context.Message);
                var result = await _mediator.Send(command);

                _logger.LogInformation("CartCheckoutEvent consumed successfully. Created Order Id: {newOrderId}", result);
            }
        }

        private IEnumerable<string> ExtractTraceContextFromConsumeContext(ConsumeContext context, string key)
        {
            try
            {
                string value = context?.Headers.Get<string>(key);
                _logger.LogDebug($"Extracting headers {key}:{value} when processing cart checkout event tracing");
                if (!string.IsNullOrEmpty(value))
                {
                    return new[] { value };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to extract trace context: {ex}");
            }

            return Enumerable.Empty<string>();
        }

        private void AddActivityTags(Activity activity)
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.rabbitmq.queue", EventBusConstants.CartCheckoutQueue);
        }
    }
}