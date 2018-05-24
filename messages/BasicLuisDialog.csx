#load "Utils.csx"
#load "CustomerSalesDataDialog.csx"

using System;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

[Serializable]
public class BasicLuisDialog : LuisDialog<object>
{
	public BasicLuisDialog () : base (new LuisService (new LuisModelAttribute (
		ConfigurationManager.AppSettings ["LuisAppId"],
		ConfigurationManager.AppSettings ["LuisAPIKey"],
		domain: ConfigurationManager.AppSettings ["LuisAPIHostName"])))
	{
	}

	[LuisIntent ("None")]
	public async Task NoneIntent (IDialogContext context, LuisResult result)
	{
		await ShowFailedLuisIntentResult (context, result);
	}

	[LuisIntent ("Greeting")]
	public async Task GreetingIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"Greetings!");

		await HelpIntent (context, result);
	}

	[LuisIntent ("Cancel")]
	public async Task CancelIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"Ok. I'm here if you need me.");
		context.Wait (MessageReceived);
	}

	[LuisIntent ("Help")]
	public async Task HelpIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"I'm the AdventureWorks Sales Bot. You can ask me things like \"Who bought the most Touring Tire in March?\"");
		context.Wait (MessageReceived);
	}

	[LuisIntent ("Products")]
	public async Task ProductsIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"Here are some of the great products we sell.");
		var replyMessage = context.MakeMessage ();
		replyMessage.TextFormat = "markdown";
		replyMessage.Text = $"Vest\nGloves\n\nTire\n\nWater Bottle\n\nSocks\n\nRoad Tire\n\nMountain Tire\n\nShorts\n\nTouring Tire\n\nJersey";
		await context.PostAsync(replyMessage);
		context.Wait (MessageReceived);
	}

	[LuisIntent ("TopCustomersForProduct")]
	public Task TopCustomersForProductIntent (IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
	{
		context.Call (new CustomerSalesDataDialog (result), ResumeAfterDialog);

		return Task.Delay (0);
	}

	private async Task ShowFailedLuisIntentResult (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"I couldn't decide what you wanted me to do based on that request. Try again with a bit more information and please be patient with me as I continue to learn.");
		await context.PostAsync ($"Try typing 'help' to see the queries I'm able to respond to.");

		context.Wait (MessageReceived);
	}

	// This function is called after each dialog process is done
	private async Task ResumeAfterDialog (IDialogContext context, IAwaitable<object> result)
	{
		var dialogResult = await result;

		// fall back to this dialog handling something if a child dialog doesn't understand - e.g. if the user repeats the query that started a CustomerSalesDataDialog
		if (dialogResult is IMessageActivity)
		{
			await MessageReceived (context, new AwaitableFromItem<IMessageActivity> (dialogResult as IMessageActivity));
		}
		else // not a msg, but we'll return control to this dialog
		{
			// MessageRecieved function will receive users' messages
			context.Wait (MessageReceived);
		}
	}
}