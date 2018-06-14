using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CycleShopBot
{
	/// <summary>
	/// A base Dialog class used when your bot needs to handle postback events from Adaptive Cards, other cards, and entered message text.
	/// </summary>
	/// <typeparam name="TResult">The type that this Dialog returns.</typeparam>
	[Serializable]
	public abstract class BotActionDialog<TResult> : IDialog<TResult>
	{
		readonly List<BotAction> Actions = new List<BotAction> ();

		protected BotAction DefineAction (string type, params string[] dataMembers)
		{
			var action = BotAction.Define (type, dataMembers);
			Actions.Add (action);

			return action;
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
				var cmd = Actions.Select (a => a.Commands.FirstOrDefault (c => c.IsCommandLabel (message.Text))).FirstOrDefault ();

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
}