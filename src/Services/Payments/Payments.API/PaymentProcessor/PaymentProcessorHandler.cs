using MediatR;

namespace Payments.API.PaymentProcessor
{
	public class PaymentProcessorHandler : IRequestHandler<PaymentProcessorCommand, bool>
	{
		private readonly ILogger<PaymentProcessorHandler> _logger;

		public PaymentProcessorHandler(ILogger<PaymentProcessorHandler> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<bool> Handle(PaymentProcessorCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation($"Payment successfully executed.");
			return true;
		}
	}
}