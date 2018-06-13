using AdaptiveCards;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;

namespace CycleShopBot.Cards
{
	public class AdaptiveSalesCard : AdaptiveCard
	{
		public AdaptiveSalesCard (string product, int numberCustomers, int month, List<CustomerSales> salesData, BotCommand<int> prevMonth, BotCommand<int> nextMonth)
		{
			var facts = new List<AdaptiveFact> ();

			foreach (var customerSalesData in salesData)
			{
				facts.Add (new AdaptiveFact (customerSalesData.Customer, String.Format ("{0:C}", customerSalesData.TotalSales)));
			}

			Body = new List<AdaptiveElement> ()
			{
				new AdaptiveContainer ()
				{
					Items =
						{
							new AdaptiveTextBlock()
							{
								Text = $"{Utils.GetMonthName(month)} - {product.ToTitleCase()}",
								Size = AdaptiveTextSize.ExtraLarge,
								Weight = AdaptiveTextWeight.Bolder
							},
							new AdaptiveFactSet()
							{
								Facts = facts
							}
						}
				}
			};

			Actions.Add (
				new AdaptiveSubmitAction ()
				{
					Title = prevMonth.Label,
					DataJson = prevMonth.RenderActionEvent ()
				}
			);

			Actions.Add (
				new AdaptiveSubmitAction ()
				{
					Title = nextMonth.Label,
					DataJson = nextMonth.RenderActionEvent ()
				}
			);
		}

		public Attachment AsAttachment ()
		{
			return new Attachment ()
			{
				ContentType = ContentType,
				Content = this
			};
		}
	}
}