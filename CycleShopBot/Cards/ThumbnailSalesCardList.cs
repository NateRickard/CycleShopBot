using CycleShopBot.Framework;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;

namespace CycleShopBot.Cards
{
	public class ThumbnailSalesCardList : List<ThumbnailCard>
	{
		public ThumbnailSalesCardList (string product, int numberCustomers, int month, List<CustomerSales> salesData, BotCommand<int> prevMonth, BotCommand<int> nextMonth)
		{
			var titleCard = new ThumbnailCard ()
			{
				Title = $"{Utils.GetMonthName (month)} - {product.ToTitleCase ()}"
			};

			Add (titleCard);

			foreach (var customerSalesData in salesData)
			{
				var card = new ThumbnailCard ()
				{
					Title = $"{customerSalesData.Customer}",
					Subtitle = $"{customerSalesData.TotalSales:C}"
				};

				Add (card);
			}

			var cardButtons = new List<CardAction> ();

			var prevButton = new CardAction ()
			{
				Value = prevMonth.RenderActionEvent (),
				Type = "postBack",
				Title = prevMonth.Label
			};

			cardButtons.Add (prevButton);

			var nextButton = new CardAction ()
			{
				Value = nextMonth.RenderActionEvent (),
				Type = "postBack",
				Title = nextMonth.Label
			};

			cardButtons.Add (nextButton);

			var buttonCard = new ThumbnailCard ()
			{
				Buttons = cardButtons
			};

			Add (buttonCard);
		}

		public List<Attachment> AsAttachmentList ()
		{
			return new List<Attachment> (this.Select (c => c.ToAttachment ()));
		}
	}
}