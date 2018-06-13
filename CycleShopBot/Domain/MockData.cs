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
			new CustomerSales { Customer = "Robert Leggo", TotalSales = 129357345.34543543M },
			new CustomerSales { Customer = "Scooter Gennett", TotalSales = 178468373.238953205M },
			new CustomerSales { Customer = "Homer Bailey", TotalSales = 123456789.34958743857M },
			new CustomerSales { Customer = "Billy Hamilton", TotalSales = 286306846.34598734785M },
			new CustomerSales { Customer = "Joey Votto", TotalSales = 592756832.34587343435M },
			new CustomerSales { Customer = "Eugenio Suarez", TotalSales = 385937568.3859473632M },
			new CustomerSales { Customer = "Tucker Barnhart", TotalSales = 524756383.5937474455M }
		};

		public static readonly List<EmployeeItem> Employees = new List<EmployeeItem> ()
		{
			new EmployeeItem { EmployeeKey = 1, FirstName = "John", LastName = "Doe", EmailAddress = "john.doe@mycorp.com", Title = "Supervisor", VacationHours = 60 },
			new EmployeeItem { EmployeeKey = 2, FirstName = "Jane", LastName = "Foe", EmailAddress = "jane.foe@hercorp.com", Title = "Purchasing Manager", VacationHours = 40 },
			new EmployeeItem { EmployeeKey = 3, FirstName = "Ricky", LastName = "Rubio", EmailAddress = "ricky.rubio@jazz.com", Title = "Point Person", VacationHours = 50 },
			new EmployeeItem { EmployeeKey = 4, FirstName = "Kyle", LastName = "Lowry", EmailAddress = "kyle.lowry@raptors.com", Title = "Senior Sales Rep", VacationHours = 80 },
			new EmployeeItem { EmployeeKey = 5, FirstName = "James", LastName = "Harden", EmailAddress = "james.harden@rockets.com", Title = "Bearded One", VacationHours = 70 },
			new EmployeeItem { EmployeeKey = 6, FirstName = "Steph", LastName = "Curry", EmailAddress = "steph.curry@warriors.com", Title = "Chief Shooter", VacationHours = 40 },
			new EmployeeItem { EmployeeKey = 7, FirstName = "LeBron", LastName = "James", EmailAddress = "lebron.james@cavs.com", Title = "GOAT", VacationHours = 120 },
			new EmployeeItem { EmployeeKey = 8, FirstName = "John", LastName = "Wall", EmailAddress = "john.wall@wizards.com", Title = "Fastest PG", VacationHours = 100 },
			new EmployeeItem { EmployeeKey = 9, FirstName = "Demarcus", LastName = "Cousins", EmailAddress = "demarcus.cousins@pelicans.com", Title = "Chief Bully", VacationHours = 90 },
			new EmployeeItem { EmployeeKey = 10, FirstName = "Anthony", LastName = "Davis", EmailAddress = "anthony.davis@pelicans.com", Title = "The Brow", VacationHours = 110 }
		};
	}
}