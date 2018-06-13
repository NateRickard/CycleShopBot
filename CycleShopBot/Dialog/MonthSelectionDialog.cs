using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace CycleShopBot
{
	[Serializable]
	public class MonthSelectionDialog : IDialog<int>
	{
		readonly DateTime minDate;
		readonly DateTime maxDate;

		public MonthSelectionDialog (DateTime minDate, DateTime maxDate)
		{
			this.minDate = minDate;
			this.maxDate = maxDate;
		}

		public async Task StartAsync (IDialogContext context)
		{
			await showMonthPrompt (context, minDate, maxDate);
		}

		private async Task showMonthPrompt (IDialogContext context, DateTime dateRangeMin, DateTime dateRangeMax)
		{
			await context.PostAsync ($"I'll need you to select the month as well.");

			var monthOptions = new List<string> ();

			DateTime dtNext = dateRangeMin;

			while (dtNext < dateRangeMax)
			{
				monthOptions.Add (Utils.GetMonthName (dtNext.Month));
				dtNext = dtNext.AddMonths (1);
			}

			PromptDialog.Choice<string> (
				context,
				AfterMenuSelection,
				monthOptions,
				"Which month would you like to view sales data for?",
				"I'll need you to give me a valid month.",
				3,
				PromptStyle.Auto,
				monthOptions);
		}

		private async Task AfterMenuSelection (IDialogContext context, IAwaitable<string> result)
		{
			try
			{
				var monthName = await result;
				var selectedMonth = Utils.GetMonthNumber (monthName);

				await context.PostAsync ($"Great, I'll show you results for {monthName}");

				context.Done (selectedMonth);
			}
			catch (TooManyAttemptsException ex)
			{
				context.Fail (ex);
			}
		}
	}
}