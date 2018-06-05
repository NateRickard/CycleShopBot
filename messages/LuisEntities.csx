using System;

public class LuisEntities
{
	public static class BuiltIn
	{
		public static LuisEntity DateRange = new LuisEntity ("builtin.datetimeV2.daterange", "daterange");

		public static LuisEntity Number = new LuisEntity ("builtin.number");
	}

	public static LuisEntity Products = new LuisEntity ("Products");

	public static LuisEntity Regions = new LuisEntity ("regions");
}

public class LuisEntity
{
	public string Identifier { get; set; }

	public string Name { get; set; }

	public LuisEntity (string identifier, string name = null)
	{
		this.Identifier = identifier;
		this.Name = name;
	}

	public static bool operator == (string e1, LuisEntity e2)
	{
		return e1 == e2.Identifier;
	}

	public static bool operator != (string e1, LuisEntity e2)
	{
		return e1 != e2.Identifier;
	}

	public override bool Equals (object obj)
	{
		if (obj is string)
		{
			return this.Identifier == obj.ToString ();
		}

		return base.Equals (obj);
	}

	public override int GetHashCode ()
	{
		return this.Identifier.GetHashCode ();
	}
}