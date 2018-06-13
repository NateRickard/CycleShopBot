using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace CycleShopBot
{
	[Serializable]
	public class RegionSelectionDialog : IDialog<string>
	{
		readonly List<string> regions = new List<string> (new string [] {
			"Northwest", "Northeast", "Central", "Southwest", "Southeast", "Canada", "France", "Germany", "UK" });

		public RegionSelectionDialog ()
		{
		}

		public async Task StartAsync (IDialogContext context)
		{
			await showRegionPrompt (context);
		}

		private async Task showRegionPrompt (IDialogContext context)
		{
			await context.PostAsync ("For which region?");

			PromptDialog.Choice<string> (
				context,
				AfterMenuSelection,
				regions,
				"Region?",
				"That's not going to work...can you try again?",
				3,
				PromptStyle.Auto,
				regions);
		}

		private async Task AfterMenuSelection (IDialogContext context, IAwaitable<string> result)
		{
			try
			{
				var regionName = await result;

				context.Done (regionName);
			}
			catch (TooManyAttemptsException ex)
			{
				context.Fail (ex);
			}
		}
	}
}