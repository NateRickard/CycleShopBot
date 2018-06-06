#load "BotAction.csx"

using System;
using System.Linq;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;

[Serializable]
public abstract class BotActionDialog<TResult> : IDialog<TResult>
{
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
		else if (message.Type == ActivityTypes.Message && (message.Text?.StartsWith ("{") ?? false)) // is this msg possibly json from a CardAction?
		{
			dynamic value = JsonConvert.DeserializeObject<dynamic> (message.Text);

			await ProcessPostBackAction (context, value);
		}
		// some channels don't support these very well and just send back the button text
		else if (message.Type == ActivityTypes.Message)
		{
			// is this a command the current dialog has defined??
			var eventJson = RenderCommandEventForLabel (message.Text);

			if (eventJson != null)
			{
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

	protected virtual string RenderCommandEventForLabel (string label)
	{
		return null;
	}

	protected async virtual Task ProcessPostBackAction (IDialogContext context, dynamic value)
	{
		// override in dialogs to handle the postback action
		await Task.Delay (0);
	}
}