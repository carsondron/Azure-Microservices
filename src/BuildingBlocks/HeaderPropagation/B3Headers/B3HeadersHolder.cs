using HeaderPropagation.B3Headers.Interfaces;

namespace HeaderPropagation.B3Headers
{
	public class B3HeadersHolder : IB3HeadersHolder
	{
        public string RequestId { get; set; }
        public string B3TraceId { get; set; }
        public string B3SpanId { get; set; }
        public string B3ParentSpanId { get; set; }
        public string B3Sampled { get; set; }
        public string B3Flags { get; set; }
        public string OtSpanContext { get; set; }
    }
}