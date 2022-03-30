using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

namespace TelemetryKitchenSink
{
	public class TelemetryBaggageLogger : BaseProcessor<Activity>
	{
		private readonly ILogger<TelemetryBaggageLogger> _logger;
        private readonly IHttpContextAccessor _context;

        public TelemetryBaggageLogger(ILogger<TelemetryBaggageLogger> logger, IHttpContextAccessor context)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override void OnStart(Activity activity)
        {
            _logger.LogDebug("-------------------> Logging Baggages In BaseProcessor:OnStart <-------------------");
            var baggageEnumerator = activity?.Baggage.GetEnumerator();
            while (baggageEnumerator.MoveNext())
            {
                _logger.LogDebug($"OnStart->Received baggage {baggageEnumerator.Current.Key}:{baggageEnumerator.Current.Value}");
                if (!_context.HttpContext.Request.Headers.Any(h => h.Key.Equals(baggageEnumerator.Current.Key, StringComparison.InvariantCultureIgnoreCase))) {
                    _context.HttpContext.Request?.Headers.Add(baggageEnumerator.Current.Key, baggageEnumerator.Current.Value);
                }
            }
        }

        public override void OnEnd(Activity activity)
        {
            _logger.LogDebug("-------------------> Logging Baggages In BaseProcessor:OnEnd <-------------------");
            var baggageEnumerator = activity?.Baggage.GetEnumerator();
            while (baggageEnumerator.MoveNext())
            {
                _logger.LogDebug($"OnEnd->Received baggage {baggageEnumerator.Current.Key}:{baggageEnumerator.Current.Value}");
                activity?.SetTag(baggageEnumerator.Current.Key, baggageEnumerator.Current.Value);
            }
        }
    }
}