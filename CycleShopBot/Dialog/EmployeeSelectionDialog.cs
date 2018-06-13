using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace CycleShopBot
{
	[Serializable]
	public class EmployeeSelectionDialog : IDialog<string>
	{
		private List<EmployeeItem> employees;

		public EmployeeSelectionDialog (List<EmployeeItem> employeeList)
		{
			this.employees = employeeList;
		}

		public async Task StartAsync (IDialogContext context)
		{
			await showRegionPrompt (context);
		}

		private async Task showRegionPrompt (IDialogContext context)
		{
			PromptDialog.Choice<string> (
				context,
				AfterMenuSelection,
				employees.Select (x => x.EmployeeKey.ToString ()),
				"Employees",
				"That's not going to work...can you try again?",
				3,
				PromptStyle.Auto,
				employees.Select (x => x.FullName));

			await Task.Delay (0);
		}

		private async Task AfterMenuSelection (IDialogContext context, IAwaitable<string> result)
		{
			try
			{
				var employeeID = await result;

				context.Done (employeeID);
			}
			catch (TooManyAttemptsException ex)
			{
				context.Fail (ex);
			}
		}
	}
}