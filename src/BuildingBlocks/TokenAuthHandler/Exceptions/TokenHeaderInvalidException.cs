namespace TokenAuth.Exceptions
{
	public class TokenHeaderInvalidException : Exception
	{
		public TokenHeaderInvalidException()
		{
		}

		public TokenHeaderInvalidException(string message)
			: base(message)
		{
		}
	}
}