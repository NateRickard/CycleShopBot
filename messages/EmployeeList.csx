#load "Utils.csx"
#load "EmployeeItem.csx"

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

[Serializable]
public class EmployeeList : IDialog<IMessageActivity>
{
    [NonSerialized]
    LuisResult luisResult;

    public EmployeeList(LuisResult luisResult)
    {
        this.luisResult = luisResult;
    }

    public async Task StartAsync(IDialogContext context)
    {
        var regions = findRegion(luisResult);
       
        // try to find an exact product match
        if (regions > 0)
        {
            await Utils.SendTypingIndicator(context);
            var employees = await GetEmployeeListForRegion(regions);
            if (employees.Count > 0)
            {
                var replyMessage = context.MakeMessage();
                replyMessage.TextFormat = "markdown";
                foreach (var emp in employees)
                {
                    replyMessage.Text += $"{emp.FirstName} {emp.LastName}\n\n";
                }
                await context.PostAsync(replyMessage);
                context.Wait(MessageReceived);
            }
        }
        else
        {
            context.Fail(new Exception("No employees for that region were found"));
        }
    }
    private async Task<List<EmployeeItem>> GetEmployeeListForRegion(int region)
    {
        using (var client = new HttpClient())
        {
            string functionSecret = ConfigurationManager.AppSettings["EmployeeListAPIKey"];
        
            var functionUri = $"https://sapbotdemo-2018.sapbotase.p.azurewebsites.net/api/SalesPeopleInRegion?code={functionSecret}";
            functionUri += $"&region={region}";

            var response = await client.PostAsync(functionUri, null);

            using (HttpContent content = response.Content)
            {
                // read the response as a string
                var jsonRaw = await content.ReadAsStringAsync();
                // clean up nasty formatted json
                var json = jsonRaw.Trim('"').Replace(@"\", string.Empty);

                return JsonConvert.DeserializeObject<List<EmployeeItem>>(json, Settings);
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

        switch (submitType)
        {
            //case "MonthChange":
            //    int selectedMonth = Convert.ToInt32(value.Month);
            //    await ShowCustomerSalesTotals(context, new AwaitableFromItem<int>(selectedMonth));
            //    return;
        }
    }

    private int findRegion(LuisResult result)
    {
        string region = "0";
        var regions = new List<string>();
        var allRegionEntities = result.Entities?.Where(e => e.Type == "regions").ToList() ?? new List<EntityRecommendation>();

        // identify a single region entity
        foreach (var entity in allRegionEntities)
        {
            if (entity.Resolution != null && entity.Resolution.TryGetValue("values", out object values))
            {
                var regionSynonyms = (values as List<object>).Where(v => v is string).Select(v => v as string).ToList();

                // if we have an exact match, this is the entity we want
                if (regionSynonyms.Count == 1)
                {
                    region = regionSynonyms[0];
                    break;
                }
            }
        }

        return int.Parse(region);
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
