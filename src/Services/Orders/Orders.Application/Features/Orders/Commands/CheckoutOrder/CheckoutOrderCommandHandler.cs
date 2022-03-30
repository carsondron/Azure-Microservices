using System.Diagnostics;
using AutoMapper;
using EventBusMessages.Common;
using EventBusMessages.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using Orders.Application.Contracts.Persistence;
using Orders.Application.GrpcServices.Interfaces;
using Orders.Domain.Entities;

namespace Orders.Application.Features.Orders.Commands.CheckoutOrder
{
    public class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, int>
    {
        private static readonly ActivitySource Activity = new(nameof(CheckoutOrderCommandHandler));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private readonly IOrderRepository _orderRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;
        private readonly INotificationsGrpcService _notificationsService;
        private readonly ILogger<CheckoutOrderCommandHandler> _logger;

        public CheckoutOrderCommandHandler(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint,
            IMapper mapper, INotificationsGrpcService notificationsService, ILogger<CheckoutOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationsService = notificationsService ?? throw new ArgumentNullException(nameof(notificationsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
        {
            using (var activity = Activity.StartActivity("Payment Execution", ActivityKind.Producer))
            {
                var orderEntity = _mapper.Map<OrderItem>(request);
                var newOrder = await _orderRepository.AddAsync(orderEntity);

                _logger.LogInformation($"Order {newOrder.Id} is successfully created.");


                // Send notification via gRPC
                await SendOrderConfirmation(newOrder);
                activity?.AddEvent(new ActivityEvent("Sent notification via gRPC"));

                // Publish via RabbitMQ
                var eventMessage = _mapper.Map<PaymentExecutionEvent>(request);
                await _publishEndpoint.Publish(eventMessage, c => AddActivityToHeader(activity, c));
                activity?.AddEvent(new ActivityEvent("Sent payment execution event via RabbitMQ"));

                return newOrder.Id;
            }
        }

        private async Task SendOrderConfirmation(OrderItem order)
        {
            string email = "joe.bloggs@example.com";
            string body = $"Order {order.Id} was created.";
            string subject = "Order was created";

            try
            {
                await _notificationsService.SendEmail(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Order {order.Id} failed due to an error with the notifications service: {ex.Message}");
            }
        }

        private void AddActivityToHeader(Activity activity, PublishContext context)
        {
            PropagationContext propagationContext = new PropagationContext(activity.Context, Baggage.Current);
            Propagator.Inject(propagationContext, context, InjectContextIntoHeader);
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.rabbitmq.queue", EventBusConstants.PaymentProcessorQueue);
        }

        private void InjectContextIntoHeader(PublishContext context, string key, string value)
        {
            try
            {
                _logger.LogDebug($"Setting header {key}:{value} for payment execution event tracing");
                context?.Headers.Set(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to inject trace context.");
            }
        }
    }
}