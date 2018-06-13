using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace CycleShopBot
{
	[Serializable]
	public class ProductSelectionDialog : IDialog<string>
	{
		readonly List<string> products;

		public ProductSelectionDialog (List<string> products)
		{
			this.products = products;
		}

		public async Task StartAsync (IDialogContext context)
		{
			await ShowProductPrompt (context);
		}

		private async Task ShowProductPrompt (IDialogContext context)
		{
			await context.PostAsync ("I found more than one matching product.");

			PromptDialog.Choice (
				context,
				AfterMenuSelection,
				products,
				"Which product would you like to view sales data for?",
				"Try entering a valid product next time! ;)",
				3,
				PromptStyle.Auto,
				products);
		}

		private async Task AfterMenuSelection (IDialogContext context, IAwaitable<string> result)
		{
			try
			{
				var productName = await result;

				await context.PostAsync ($"Great, I'll show you results for {productName}");

				context.Done (productName);
			}
			catch (TooManyAttemptsException ex)
			{
				context.Fail (ex);
			}
		}
	}
}