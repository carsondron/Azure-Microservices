using System.Diagnostics;
using AutoMapper;
using EventBusMessages.Common;
using EventBusMessages.Events;
using MassTransit;
using MediatR;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Payments.API.PaymentProcessor;

namespace Payments.API.EventBusConsumer
{
	public class PaymentExecutionConsumer : IConsumer<PaymentExecutionEvent>
    {
        private static readonly ActivitySource Activity = new(nameof(PaymentExecutionConsumer));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentExecutionConsumer> _logger;

        public PaymentExecutionConsumer(IMediator mediator, IMapper mapper, ILogger<PaymentExecutionConsumer> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Consume(ConsumeContext<PaymentExecutionEvent> context)
        {
            var parentContext = Propagator.Extract(default, context, ExtractTraceContextFromConsumeContext);
            Baggage.Current = parentContext.Baggage;

            using (var activity = Activity.StartActivity("Process Payment Execution Event", ActivityKind.Consumer, parentContext.ActivityContext))
            {
                AddActivityTags(activity);
                var command = _mapper.Map<PaymentProcessorCommand>(context.Message);
                await _mediator.Send(command);

                _logger.LogInformation("PaymentExecutionEvent consumed successfully");
            }
        }

        private IEnumerable<string> ExtractTraceContextFromConsumeContext(ConsumeContext context, string key)
        {
            try
            {
                string value = context?.Headers.Get<string>(key);
                _logger.LogDebug($"Extracting headers {key}:{value} when processing payment execution event tracing");
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
            activity?.SetTag("messaging.rabbitmq.queue", EventBusConstants.PaymentProcessorQueue);
        }
    }
}