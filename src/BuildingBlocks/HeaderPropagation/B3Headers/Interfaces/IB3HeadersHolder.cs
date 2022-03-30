namespace HeaderPropagation.B3Headers.Interfaces
{
	public interface IB3HeadersHolder
	{
        string RequestId { get; set; }
        string B3TraceId { get; set; }
        string B3SpanId { get; set; }
        string B3ParentSpanId { get; set; }
        string B3Sampled { get; set; }
        string B3Flags { get; set; }
        string OtSpanContext { get; set; }
    }
}