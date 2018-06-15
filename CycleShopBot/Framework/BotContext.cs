using CycleShopBot.Framework;
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

		public static Channel CurrentChannel { get; private set; }

		public static void Initialize (HttpRequestMessage req, Activity activity)
		{
			if (BaseFunctionUrl == null)
			{
				BaseFunctionUrl = req.RequestUri.AbsoluteUri.Replace (req.RequestUri.PathAndQuery, string.Empty);
			}

			CurrentChannel = Channel.FromId (activity.ChannelId);
		}
	}
}