using System;

namespace CycleShopBot
{
	public class CycleShopException : ApplicationException
	{
		CycleShopException (string msg) : base (msg) { }

		public static CycleShopException From (string msg)
		{
			return new CycleShopException (msg);
		}
	}

	/// <summary>
	/// These are our handled exceptions, i.e. exceptions we will catch and respond to/handle at the point they are caught.  
	/// All other, unhandled exceptions will be handled generically in the main dialog.
	/// </summary>
	public static class Exceptions
	{
		public static readonly CycleShopException TooManyProductsMatched = CycleShopException.From ("Too many products matched.");

		public static readonly CycleShopException DataException = CycleShopException.From ("There was an issue retrieving data.");
	}
}