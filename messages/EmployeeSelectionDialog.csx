#load "Utils.csx"

using System;
using System.Linq;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

[Serializable]
public class EmployeeSelectionDialog : IDialog<string>
{
    private List<EmployeeItem> employees;

    public EmployeeSelectionDialog(List<EmployeeItem> employeeList)
    {
        this.employees = employeeList;
    }

    public async Task StartAsync(IDialogContext context)
    {
        await showRegionPrompt(context);
    }

    private async Task showRegionPrompt(IDialogContext context)
    {
        PromptDialog.Choice<string>(
            context,
            AfterMenuSelection,
            employees.Select(x => x.EmployeeKey.ToString()),
            "Employees",
            "That's not going to work...can you try again?",
            3,
            PromptStyle.Auto,
            employees.Select(x => x.FullName));
    }

    private async Task AfterMenuSelection(IDialogContext context, IAwaitable<string> result)
    {
        var employeeID = await result;

        context.Done(employeeID);
    }
}