using System;
using System.Globalization;

public static class Utils
{
	public static string GetMonthName (int month)
	{
		return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName (month);
	}

	public static int GetMonthNumber (string monthName)
	{
		return Convert.ToDateTime ("01-" + monthName + "-2018").Month;
	}

	public static string ToTitleCase (string str)
	{
		TextInfo textInfo = new CultureInfo ("en-US", false).TextInfo;

		return textInfo.ToTitleCase (str);
	}
}