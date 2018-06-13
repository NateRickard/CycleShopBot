using System.Collections.Generic;

namespace CycleShopBot
{
	public class MockData
    {
		public static readonly List<CustomerSales> CustomerSales = new List<CustomerSales> ()
		{
			new CustomerSales { Customer = "John Doe", TotalSales = 838923489.32347829M },
			new CustomerSales { Customer = "Luis Castillo", TotalSales = 456895498.634265479M },
			new CustomerSales { Customer = "Wandy Peralta", TotalSales = 323475928.23874824444M },
			new CustomerSales { Customer = "Jane Doe", TotalSales = 243859563.348580953M },
			new CustomerSales { Customer = "Robert Leggo", TotalSales = 129357345.34543543M }
		};
	}
}