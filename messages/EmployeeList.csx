#load "Utils.csx"
#load "EmployeeItem.csx"
#load "RegionSelectionDialog.csx"
#load "EmployeeSelectionDialog.csx"

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
public class EmployeeList : IDialog<IMessageActivity>
{
    string selectedRegion = null;
    string selectedEmployeeID = null;
    List<EmployeeItem> employees;

    [NonSerialized]
    LuisResult luisResult;

    

    public EmployeeList(LuisResult luisResult)
    {
        this.luisResult = luisResult;
    }

    public async Task StartAsync(IDialogContext context)
    {
        var regionCode = findRegion(luisResult);
       
        // try to find an exact product match
        if (regionCode > 0)
        {
            await DisplayEmployeeList(context, regionCode, "");
        }
        else
        {
            context.Call(new RegionSelectionDialog(), RegionSelected);
        }
    }

    private async Task RegionSelected(IDialogContext context, IAwaitable<string> regionResult)
    {
        selectedRegion = await regionResult;
       
        await DisplayEmployeeList(context, 0, selectedRegion);

    }

    private async Task EmployeeSelected(IDialogContext context, IAwaitable<string> employeeResult)
    {
        selectedEmployeeID = await employeeResult;

        await DisplayEmployeeCard(context, selectedEmployeeID);
    }

    /// <summary>
    /// Must have either regionCode or region supplied.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="regionCode"></param>
    /// <param name="region"></param>
    /// <returns></returns>
    private async Task DisplayEmployeeList(IDialogContext context, int regionCode, string region)
    {
        await Utils.SendTypingIndicator(context);
        employees = await GetEmployeeListForRegion(regionCode, region);
        if (employees.Count > 0)
        {
            context.Call(new EmployeeSelectionDialog(employees), EmployeeSelected);
        }
    }

    private async Task DisplayEmployeeCard(IDialogContext context, string employeeId)
    {
        await Utils.SendTypingIndicator(context);

        var employee = employees.Where(x => x.EmployeeKey == long.Parse(employeeId)).First();

        var replyToConversation = context.MakeMessage();
        replyToConversation.Attachments = new List<Attachment>();
        AdaptiveCard card = new AdaptiveCard();
            
        // Add text to the card.
        card.Body.Add(new AdaptiveTextBlock()
        {
            Text = employee.FullName,
            Size = AdaptiveTextSize.Large,
            Weight = AdaptiveTextWeight.Bolder
        });

        // Add text to the card.
        card.Body.Add(new AdaptiveTextBlock()
        {
            Text = employee.Title
        });

        // Add text to the card.
        card.Body.Add(new AdaptiveTextBlock()
        {
            Text = $"Vacation: {employee.VacationHours}"
        });

        // Add buttons to the card.
        card.Actions.Add(new AdaptiveOpenUrlAction()
        {
            Url = new Uri($"email:{employee.EmailAddress}"),
            Title = $"Email {employee.FirstName}"
        });

        card.Actions.Add(new AdaptiveOpenUrlAction()
        {
            Url = new Uri("phone:{employee.Phone}"),
            Title = $"Call {employee.FirstName}"
        });

        card.Actions.Add(new AdaptiveOpenUrlAction()
        {
            Url = new Uri($"sms:{employee.Phone}"),
            Title = $"Text {employee.FirstName}"
        });

        replyToConversation.Attachments.Add(new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card }) ;
        await context.PostAsync(replyToConversation);
        context.Wait(MessageReceived);
    }

    private async Task<List<EmployeeItem>> GetEmployeeListForRegion(int regionCode, string region)
    {
        using (var client = new HttpClient())
        {
            string functionSecret = ConfigurationManager.AppSettings["EmployeeListAPIKey"];
        
            var functionUri = $"https://sapbotdemo-2018.sapbotase.p.azurewebsites.net/api/SalesPeopleInRegion?code={functionSecret}";
            functionUri += $"&regionCode={regionCode}&region={region}";

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
