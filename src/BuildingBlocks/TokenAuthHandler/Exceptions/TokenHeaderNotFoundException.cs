namespace TokenAuth.Exceptions
{
	public class TokenHeaderNotFoundException : Exception
	{
		public TokenHeaderNotFoundException()
		{
		}

		public TokenHeaderNotFoundException(string message)
			: base(message)
		{
		}
	}
}