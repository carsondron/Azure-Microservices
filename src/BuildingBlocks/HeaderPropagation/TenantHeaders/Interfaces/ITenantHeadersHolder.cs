namespace HeaderPropagation.TenantHeaders.Interfaces
{
	public interface ITenantHeadersHolder
	{
		string TenantId { get; set; }
		string UserId { get; set; }
	}
}