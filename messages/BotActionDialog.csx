#load "BotAction.csx"
#load "Command.csx"

using System;
using System.Linq;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;

/// <summary>
/// A base Dialog class used when your bot needs to handle postback events from Adaptive Cards, other cards, and entered message text.
/// </summary>
/// <typeparam name="TResult">The type that this Dialog returns.</typeparam>
[Serializable]
public abstract class BotActionDialog<TResult> : IDialog<TResult>
{
	List<Command> Commands = new List<Command> ();

	protected Command DefineCommand (string label, BotAction action)
	{
		var cmd = Command.Define (label, action);

		Commands.Add (cmd);

		return cmd;
	}

	protected Command<TOut> DefineCommand<TOut> (string label, BotAction action, Func<TOut> dataFactory)
	{
		var cmd = Command.Define (label, action, dataFactory);

		Commands.Add (cmd);

		return cmd;
	}

	protected Command<TIn, TOut> DefineCommand<TIn, TOut> (string label, BotAction action, Func<TIn, TOut> dataFactory)
	{
		var cmd = Command.Define (label, action, dataFactory);

		Commands.Add (cmd);

		return cmd;
	}

	public async virtual Task StartAsync (IDialogContext context)
	{
		await Task.Delay (0);
	}

	protected virtual async Task MessageReceived (IDialogContext context, IAwaitable<IMessageActivity> item)
	{
		var message = await item;

		if (message.Value != null) // adaptive card actions usually come thru like this
		{
			dynamic value = message.Value;

			await ProcessPostBackAction (context, value);
		}
		else if (message.Type == ActivityTypes.Message && BotAction.IsActionEvent (message.Text)) // is this msg possibly json from a CardAction?
		{
			dynamic value = JsonConvert.DeserializeObject<dynamic> (message.Text);

			await ProcessPostBackAction (context, value);
		}
		// some channels don't support these very well and just send back the button text
		else if (message.Type == ActivityTypes.Message)
		{
			// is this a command the current dialog has defined??
			var cmd = Commands.FirstOrDefault (c => c.IsCommandLabel (message.Text));

			if (cmd != null)
			{
				var eventJson = cmd.RenderActionEvent ();

				dynamic value = JsonConvert.DeserializeObject<dynamic> (eventJson);

				await ProcessPostBackAction (context, value);
			}
			else
			{
				context.Done (message);
			}
		}
		else //exit this dialog if we don't understand/handle what's coming in
		{
			context.Done (message);
		}
	}

	protected async virtual Task ProcessPostBackAction (IDialogContext context, dynamic value)
	{
		// override in dialogs to handle the postback action
		await Task.Delay (0);
	}
}