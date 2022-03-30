using HeaderPropagation.TenantHeaders.Interfaces;

namespace HeaderPropagation.TenantHeaders
{
	public class TenantHeadersHolder : ITenantHeadersHolder
	{
        public string TenantId { get; set; }
        public string UserId { get; set; }
    }
}