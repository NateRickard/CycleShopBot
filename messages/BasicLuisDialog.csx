#load "Utils.csx"
#load "CustomerSalesDataDialog.csx"
#load "EmployeeList.csx"
#load "EmployeeDetails.csx"

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
		await HelpIntent (context, result);
	}

	[LuisIntent ("Cancel")]
	public async Task CancelIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"Ok. I'm here if you need me.");
		context.Wait (MessageReceived);
	}

    [LuisIntent ("EmployeeList")]
    public Task EmployeeListIntent (IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
    {
        context.Call(new EmployeeList(result), ResumeAfterDialog);

        return Task.Delay(0);
    }

    [LuisIntent ("EmployeeDetails")]
    public Task EmployeeDetailsIntent(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
    {
        context.Call(new EmployeeDetails(result), ResumeAfterDialog);

        return Task.Delay(0);
    }

    [LuisIntent ("Help")]
	public async Task HelpIntent (IDialogContext context, LuisResult result)
	{
        var helpMessage = context.MakeMessage();
        helpMessage.Text = $"Hi! I'm the Cycle Shop Sales Bot. You can ask me things like \"Who bought the most Touring Tires in March?\"";
        helpMessage.SuggestedActions = new SuggestedActions()
        {
            Actions = new List<CardAction>()
            {
                new CardAction(){ Title = "Show Sample Queries", Type=ActionTypes.ImBack, Value="Show Samples" }
            }
        };

        await context.PostAsync(helpMessage);
		context.Wait (MessageReceived);
	}

    [LuisIntent ("Examples")]
    public async Task ExamplesIntent (IDialogContext context, LuisResult result)
    {
        var examplesMessage = context.MakeMessage();
        examplesMessage.Text = "Here are some sample queries";
        examplesMessage.SuggestedActions = new SuggestedActions()
        {
            Actions = new List<CardAction>()
            {
                new CardAction(){ Title = "Top Tire Sales", Type=ActionTypes.ImBack, Value="Show me top sales for Touring Tire in April" },
                new CardAction(){ Title = "Product List", Type=ActionTypes.ImBack, Value="List Products" },
                new CardAction(){ Title = "Employees in UK", Type=ActionTypes.ImBack, Value="Show employees in UK" }
            }
        };

        await context.PostAsync(examplesMessage);
        context.Wait(MessageReceived);
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


	[LuisIntent("SalesReport")]
	public async Task SalesReportIntent(IDialogContext context, LuisResult result)
	{
		var replyMessage = context.MakeMessage();
		replyMessage.Text = $"Here's the current sales report for you!";
		replyMessage.Attachments.Add(new Attachment()
			{
				ContentUrl = "https://sapcalwe64172.blob.core.windows.net/demofiles/CycleShopSales.pdf",
				ContentType = "image/png",
				Name = "CycleShopSales.pdf"
			});
		await context.PostAsync(replyMessage);
		context.Wait(MessageReceived);
	}

    [LuisIntent("Regions")]
    public async Task RegionsIntent(IDialogContext context, LuisResult result)
    {
        var replyMessage = context.MakeMessage();
        replyMessage.TextFormat = "markdown";
        replyMessage.Text = $"Cycle Shop Regions\n\n-Northwest\n-Northeast\n-Central\n-Southwest\n-Southeast\n-Canada\n-France\n-Germany\n-UK";
        await context.PostAsync(replyMessage);
        context.Wait(MessageReceived);
    }

    [LuisIntent ("TopCustomersForProduct")]
	public Task TopCustomersForProductIntent (IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
	{
		context.Call (new CustomerSalesDataDialog (result), ResumeAfterDialog);

		return Task.Delay (0);
	}

	private async Task ShowFailedLuisIntentResult (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"I couldn't decide what to do based on that input. Try again with a bit more information and please be patient with me as I continue to learn.");
		await context.PostAsync ($"Try typing 'help' to see the queries I'm able to respond to.");

		context.Wait (MessageReceived);
	}

	// This function is called after each dialog process is done
	private async Task ResumeAfterDialog (IDialogContext context, IAwaitable<object> result)
	{
		try
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
		catch
		{
			// an error occurred in the child dialog
			await ShowFailedLuisIntentResult (context, null);
		}
	}
}