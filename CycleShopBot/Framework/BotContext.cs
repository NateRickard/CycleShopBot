using System;
using System.Configuration;

namespace CycleShopBot
{
	public static class BotContext
    {
		public static readonly bool MockDataEnabled = Convert.ToBoolean (ConfigurationManager.AppSettings ["MockData"] ?? "false");
	}
}