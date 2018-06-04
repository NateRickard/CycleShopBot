#load "Utils.csx"
#load "EmployeeItem.csx"
#load "EmployeeCard.csx"


using System;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web;
using System.Globalization;
using System.Linq;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using AdaptiveCards;

[Serializable]
public class EmployeeDetails : IDialog<IMessageActivity>
{
   
    int selectedEmployeeID = 0;
   
  

    public EmployeeDetails(LuisResult luisResult)
    {
         selectedEmployeeID = findEmployee(luisResult);
    }

    public async Task StartAsync(IDialogContext context)
    {
        var data = await GetEmployeeDetails(context, selectedEmployeeID);
        await DisplayEmployeeCard(context, data);

    }

    private int findEmployee(LuisResult result)
    {
        string employeeId = "0";
        var regions = new List<string>();
        var allEmployeeEntities = result.Entities?.Where(e => e.Type == "builtin.number").ToList() ?? new List<EntityRecommendation>();

        // identify a single region entity
        foreach (var entity in allEmployeeEntities)
        {
            if (entity.Resolution != null && entity.Resolution.TryGetValue("value", out object values))
            {
                employeeId = values.ToString();
            }
        }

        return int.Parse(employeeId);
    }
   

    private async Task DisplayEmployeeCard(IDialogContext context, EmployeeItem employee)
    {
        await Utils.SendTypingIndicator(context);

        var replyToConversation = context.MakeMessage();
        replyToConversation.Attachments = new List<Attachment>();

        AdaptiveCard card = new EmployeeCard(employee);

        replyToConversation.Attachments.Add(new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card });
        await context.PostAsync(replyToConversation);
        context.Wait(MessageReceived);
    }

    private async Task<EmployeeItem> GetEmployeeDetails(IDialogContext context, int employeeid)
    {
        using (var client = new HttpClient())
        {
            var functionUri = Utils.GetFunctionUrl(context, "EmployeeDetails",
                (nameof(employeeid), employeeid));

            var response = await client.PostAsync(functionUri, null);

            using (HttpContent content = response.Content)
            {
                // read the response as a string
                var jsonRaw = await content.ReadAsStringAsync();
                // clean up nasty formatted json
                var json = jsonRaw.Trim('"').Replace(@"\", string.Empty);

                var data = JsonConvert.DeserializeObject<List<EmployeeItem>>(json, Settings);
                return data[0];
            }
        }
    }

    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };

    private async Task processPostBackAction(IDialogContext context, dynamic value)
    {
        string submitType = value.Type.ToString();

        //switch (submitType)
        //{
            //case "MonthChange":
            //    int selectedMonth = Convert.ToInt32(value.Month);
            //    await ShowCustomerSalesTotals(context, new AwaitableFromItem<int>(selectedMonth));
            //    return;
        //}

		await Task.Delay (0);
    }


    protected virtual async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
    {
        var message = await item;

        if (message.Value != null)
        {
            // Got an Action Submit!
            dynamic value = message.Value;

            await processPostBackAction(context, value);
        }
        else if (message.Type == "message" && (message.Text?.StartsWith("{") ?? false)) // is this msg possibly json from a CardAction?
        {
            dynamic value = JsonConvert.DeserializeObject<dynamic>(message.Text);

            await processPostBackAction(context, value);
        }
        else //exit this dialog if we don't understand/handle what's coming in
        {
            context.Done(message);
        }
    }
}
