#load "Utils.csx"

using System;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

[Serializable]
public class ProductSelectionDialog : IDialog<string>
{
	List<string> products;

	public ProductSelectionDialog (List<string> products)
	{
		this.products = products;
	}

	public async Task StartAsync (IDialogContext context)
	{
		await showProductPrompt (context);
	}

	private async Task showProductPrompt (IDialogContext context)
	{
		await context.PostAsync ("I found more than one matching product.");

		PromptDialog.Choice<string> (
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