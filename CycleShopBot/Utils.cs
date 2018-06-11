using System;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace CycleShopBot
{
    public static class Utils
    {
        public static string GetMonthName(int month)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }

        public static int GetMonthNumber(string monthName)
        {
            return Convert.ToDateTime("01-" + monthName + "-2018").Month;
        }

        public static string ToTitleCase(string str)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            return textInfo.ToTitleCase(str);
        }

        public static async Task SendTypingIndicator(IDialogContext context)
        {
            //Send typing indicator
            var typingIndicator = context.MakeMessage();
            typingIndicator.Type = ActivityTypes.Typing;
            await context.PostAsync(typingIndicator);
        }

        public static string GetFunctionUrl(IDialogContext context, string functionName, params (string Name, object Value)[] args)
        {
            string functionSecret = ConfigurationManager.AppSettings[$"{functionName}APIKey"];

            //context.PostAsync ("baseUrl url is:" + BaseFunctionUrl);

            if (CycleShopBot.BaseFunctionUrl.Contains("localhost")) //can't hit SAP locally so we need to hit the prod functions here
            {
                CycleShopBot.BaseFunctionUrl = ConfigurationManager.AppSettings["DefaultFunctionUrl"];
            }

            var functionUrl = $"{CycleShopBot.BaseFunctionUrl}/api/{functionName}?code={functionSecret}";

            if (args != null)
            {
                foreach (var arg in args)
                {
                    functionUrl += $"&{arg.Name}={HttpUtility.UrlEncode(arg.Value.ToString())}";
                }
            }

            return functionUrl;
        }
    }
}