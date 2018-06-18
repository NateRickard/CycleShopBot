using CycleShopBot.Framework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Configuration;
using System.Net.Http;

namespace CycleShopBot
{
	public static class BotContext
	{
		public static readonly bool MockDataEnabled = Convert.ToBoolean (ConfigurationManager.AppSettings ["MockData"] ?? "false");

		public static readonly bool LocalFunctionsEnabled = Convert.ToBoolean (ConfigurationManager.AppSettings ["LocalDataFunctions"] ?? "false");

		public static readonly string DefaultFunctionUrl = ConfigurationManager.AppSettings ["DefaultFunctionUrl"];

		public static string BaseFunctionUrl { get; set; }

		public static void Initialize (HttpRequestMessage req)
		{
			if (BaseFunctionUrl == null)
			{
				// if we are not hitting other functions locally, we need to default to their production host
				if (!LocalFunctionsEnabled && req.RequestUri.AbsoluteUri.Contains ("localhost"))
				{
					BaseFunctionUrl = DefaultFunctionUrl;
				}
				else
				{
					BaseFunctionUrl = req.RequestUri.AbsoluteUri.Replace (req.RequestUri.PathAndQuery, string.Empty);
				}
			}
		}

		public static Channel GetCurrentChannel (IDialogContext context)
		{
			return Channel.FromId (context.Activity.ChannelId);
		}
	}
}