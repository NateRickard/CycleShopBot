#load "Utils.csx"

using System;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

[Serializable]
public class RegionSelectionDialog : IDialog<string>
{
    List<string> regions = new List<string>(new string[] { "Northwest", "Northeast", "Central", "Southwest", "Southeast", "Canada",
        "France", "Germany", "UK" });
 

    public RegionSelectionDialog()
    {
        
    }

    public async Task StartAsync(IDialogContext context)
    {
        await showRegionPrompt(context);
    }

    private async Task showRegionPrompt(IDialogContext context)
    {
        await context.PostAsync("For which region?");

        PromptDialog.Choice<string>(
            context,
            AfterMenuSelection,
            regions,
            "Region?",
            "That's not going to work...can you try again?",
            3,
            PromptStyle.Auto,
            regions);
    }

    private async Task AfterMenuSelection(IDialogContext context, IAwaitable<string> result)
    {
        var regionName = await result;

        context.Done(regionName);
    }
}