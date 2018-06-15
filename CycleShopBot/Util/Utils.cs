using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;

namespace CycleShopBot
{
	public static class Utils
	{
		static TextInfo TextInfo = new CultureInfo ("en-US", false).TextInfo;

		public static string GetMonthName (int month)
		{
			return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName (month);
		}

		public static int GetMonthNumber (string monthName)
		{
			return Convert.ToDateTime ("01-" + monthName + "-2018").Month;
		}

		public static string ToTitleCase (this string str)
		{
			return TextInfo.ToTitleCase (str);
		}

		public static async Task SendTypingIndicator (this IDialogContext context)
		{
			//Send typing indicator
			var typingIndicator = context.MakeMessage ();
			typingIndicator.Type = ActivityTypes.Typing;
			await context.PostAsync (typingIndicator);
		}

		public static string GetFunctionUrl (string functionName, params (string Name, object Value) [] args)
		{
			string functionSecret = ConfigurationManager.AppSettings [$"{functionName}APIKey"];

			//context.PostAsync ("baseUrl url is:" + BaseFunctionUrl);

			if (!BotContext.LocalFunctionsEnabled && BotContext.BaseFunctionUrl.Contains ("localhost")) //can't hit SAP locally so we need to hit the prod functions here
			{
				BotContext.BaseFunctionUrl = BotContext.DefaultFunctionUrl;
			}

			var functionUrl = $"{BotContext.BaseFunctionUrl}/api/{functionName}?code={functionSecret}";

			if (args != null)
			{
				foreach (var arg in args)
				{
					functionUrl += $"&{arg.Name}={HttpUtility.UrlEncode (arg.Value.ToString ())}";
				}
			}

			return functionUrl;
		}

		public static string CleanJson (this string rawJson)
		{
			// clean up nasty formatted json
			return rawJson.Trim ('"').Replace (@"\", string.Empty);
		}
	}
}