using System;
using System.Configuration;
using System.Globalization;
using System.Web;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

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

	public static async Task SendTypingIndicator (IDialogContext context)
	{
		//Send typing indicator
		var typingIndicator = context.MakeMessage ();
		typingIndicator.Type = ActivityTypes.Typing;
		await context.PostAsync (typingIndicator);
	}

	public static string GetFunctionUrl (IDialogContext context, string functionName, params (string Name, object Value)[] args)
	{
		string functionSecret = ConfigurationManager.AppSettings [$"{functionName}APIKey"];

		//var portStart = context.Activity.ServiceUrl.LastIndexOf (':');

		var baseUrl = context.Activity.ServiceUrl;

		if (baseUrl.Contains ("localhost")) //can't hit SAP locally so we need to hit the prod functions here
		{
			baseUrl = ConfigurationManager.AppSettings ["DefaultFunctionUrl"];
		}

		var functionUrl = $"{baseUrl}/api/{functionName}?code={functionSecret}";

		if (args != null)
		{
			foreach (var arg in args)
			{
				functionUrl += $"&{arg.Name}={HttpUtility.UrlEncode (arg.Value.ToString ())}";
			}
		}

		return functionUrl;
	}
}