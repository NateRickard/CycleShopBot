using System;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;

[Serializable]
public abstract class BotActionDialog<TResult> : IDialog<TResult>
{
	public abstract Task StartAsync (IDialogContext context);

	protected virtual bool IsCommand (string message)
	{
		return false;
	}

	protected virtual string GenerateCommandEvent (string message)
	{
		throw new NotImplementedException ();
	}

	protected virtual async Task MessageReceived (IDialogContext context, IAwaitable<IMessageActivity> item)
	{
		var message = await item;

		if (message.Value != null) // adaptive cards usually come thru like this
		{
			// Got an Action Submit!
			dynamic value = message.Value;

			await ProcessPostBackAction (context, value);
		}
		else if (message.Type == "message" && (message.Text?.StartsWith ("{") ?? false)) // is this msg possibly json from a CardAction?
		{
			dynamic value = JsonConvert.DeserializeObject<dynamic> (message.Text);

			await ProcessPostBackAction (context, value);
		}
		// some channels don't support these very well and just send back the button text
		else if (message.Type == "message" && IsCommand(message.Text))
		{
			dynamic value = JsonConvert.DeserializeObject<dynamic> (GenerateCommandEvent(message.Text));

			await ProcessPostBackAction (context, value);
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