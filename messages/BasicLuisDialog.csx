#load "DecimalConverter.csx"
#load "CustomerSales.csx"
#load "MonthSelectionDialog.csx"

using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;

using AdaptiveCards;

// For more information about this template visit http://aka.ms/azurebots-csharp-luis
[Serializable]
public class BasicLuisDialog : LuisDialog<object>
{
	const int DefaultNumberCustomers = 5;
	const string Product_Name = "Product.Name";
	const string DateRange = "builtin.datetimeV2.daterange";
	const string Number = "builtin.number";

	int numberCustomers = DefaultNumberCustomers;
	string product;

	public BasicLuisDialog () : base (new LuisService (new LuisModelAttribute (
		ConfigurationManager.AppSettings ["LuisAppId"],
		ConfigurationManager.AppSettings ["LuisAPIKey"],
		domain: ConfigurationManager.AppSettings ["LuisAPIHostName"])))
	{
	}

	[LuisIntent ("None")]
	public async Task NoneIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"I couldn't understand your request. Try typing 'help' to see the queries I'm able to respond to.");
		context.Wait (MessageReceived);
	}

	[LuisIntent ("Greeting")]
	public async Task GreetingIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"Greetings!");

		await HelpIntent (context, result);
	}

	[LuisIntent ("Cancel")]
	public async Task CancelIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"Ok. I'm here if you need me.");
		context.Wait (MessageReceived);
	}

	[LuisIntent ("Help")]
	public async Task HelpIntent (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"I'm the AdventureWorks Sales Bot. You can ask me things like \"Who bought the most Touring Tire in March?\"");
		context.Wait (MessageReceived);
	}

	[LuisIntent ("TopCustomersForProduct")]
	public async Task TopCustomersForProductIntent (IDialogContext context, LuisResult result)
	{
		if (result.TryFindEntity (Product_Name, out EntityRecommendation productEntity))
		{
			product = productEntity.Entity;
			DateTime now = DateTime.Now;
			// default to the current month
			DateTime dateRangeMin = new DateTime (now.Year, now.Month, 1);
			DateTime dateRangeMax = new DateTime (now.Year, now.Month + 1, 1);
			int dateStartIndex = -1;
			int dateEndIndex = -1;

			await context.PostAsync ($"Sure, I can help you with sales data for {product}");
			//await context.PostAsync ("Getting that information now...");

			if (result.TryFindEntity (DateRange, out EntityRecommendation dateRangeEntity))
			{
				dateStartIndex = dateRangeEntity.StartIndex ?? -1;
				dateEndIndex = dateRangeEntity.EndIndex ?? -1;

				if (dateRangeEntity.Resolution != null && dateRangeEntity.Resolution.TryGetValue ("values", out object values))
				{
					//get all values in this list and find the first one that's a date range.. there may be separate years represented, etc.
					var ranges = (values as List<object>).Where (v => v is Dictionary<string, object>).Select (v => v as Dictionary<string, object>);
					var range = ranges.FirstOrDefault (v => v.ContainsValue ("daterange"));

					if (range != null)
					{
						dateRangeMin = Convert.ToDateTime (range ["start"]);
						dateRangeMax = Convert.ToDateTime (range ["end"]);
					}
				}
			}

			var numberEntities = result.Entities?.Where (e => e.Type == Number).ToList();

			while (numberEntities.Count > 0)
			{
				var numberEntity = numberEntities [0];

				// sneaky phony numbers coming back when entering dates like 1/1/2018, so we axe them here
				if (numberEntity.StartIndex >= dateStartIndex && numberEntity.EndIndex <= dateEndIndex)
				{
					numberEntities.Remove (numberEntity);
					continue;
				}

				numberCustomers = Convert.ToInt32 (numberEntity.Entity);
				break;
			}

			// if the date range seems to be more than a 1 month span we need to ket the user know that's not supported
			if (dateRangeMax.Month != dateRangeMin.Month && (dateRangeMax.Day > 1 || dateRangeMax.TimeOfDay.TotalSeconds > 1))
			{
				context.Call (new MonthSelectionDialog(dateRangeMin, dateRangeMax), ShowCustomerSalesTotals);
			}
			else
			{
				await ShowCustomerSalesTotals (context, new AwaitableFromItem<int> (dateRangeMin.Month));
			}

			return;
		}

		await this.ShowFailedLuisIntentResult (context, result);
	}

	private async Task ShowCustomerSalesTotals (IDialogContext context, IAwaitable<int> result)
	{
		var selectedMonth = await result;

		// show a typing indicator to let the user know we're working on it?

		// get SAP data with parameterized query and return it
		var data = await GetTopCustomerSalesForProduct (product, selectedMonth, numberCustomers);

		await context.PostAsync ($"It looks like these are the top {numberCustomers} customers that have purchased {product} in the month of {getMonthName(selectedMonth)}:");

		// create our reply and add the sales card attachment
		var replyMessage = context.MakeMessage ();
		var card = getSalesCard (numberCustomers, selectedMonth, data);

		// Create the attachment
		replyMessage.Attachments = new List<Attachment> {
			new Attachment()
			{
				ContentType = AdaptiveCard.ContentType,
				Content = card
			}
		};

		await context.PostAsync (replyMessage);

		context.Wait (MessageReceived);
	}

	private async Task ShowFailedLuisIntentResult (IDialogContext context, LuisResult result)
	{
		await context.PostAsync ($"You said: {result.Query}");
		await context.PostAsync ($"I couldn't decide what you wanted me to do based on that query. Try again with a bit more information and please be patient with me as I continue to learn.");
		context.Wait (MessageReceived);
	}

	private async Task<List<CustomerSales>> GetTopCustomerSalesForProduct (string product, int month, int numberCustomers = 5)
	{
		using (var client = new HttpClient ())
		{
			string functionSecret = ConfigurationManager.AppSettings ["TopCustomersForProductAPIKey"];

			// anotherFunctionUri is another Azure Function's 
			// public URL, which should provide the secret code stored in app settings 
			// with key 'AnotherFunction_secret'
			//Uri anotherFunctionUri = new Uri(req.RequestUri.AbsoluteUri.Replace(
			//	req.RequestUri.PathAndQuery, 
			//	$"/api/AnotherFunction?code={anotherFunctionSecret}"));

			var functionUri = $"https://sapbotdemo-2018.sapbotase.p.azurewebsites.net/api/TopCustomersForProduct?code={functionSecret}";
			functionUri += $"&product={HttpUtility.UrlEncode (product)}";
			functionUri += $"&month={month}";
			functionUri += $"&count={numberCustomers}";

			var response = await client.PostAsync (functionUri, null);

			using (HttpContent content = response.Content)
			{
				// read the response as a string
				var jsonRaw = await content.ReadAsStringAsync ();
				// clean up nasty formatted json
				var json = jsonRaw.Trim ('"').Replace (@"\", string.Empty);

				// settings to properly handle our scientific notation formatted decimal values
				var settings = new JsonSerializerSettings
				{
					FloatParseHandling = FloatParseHandling.Decimal,
					Converters = new List<JsonConverter> { new DecimalConverter () }
				};

				return JsonConvert.DeserializeObject<List<CustomerSales>> (json, settings);
			}
		}
	}

	private AdaptiveCard getSalesCard (int numberCustomers, int month, List<CustomerSales> salesData)
	{
		var facts = new List<AdaptiveFact> ();

		foreach (var customerSalesData in salesData)
		{
			facts.Add (new AdaptiveFact (customerSalesData.Customer, String.Format ("{0:C}", customerSalesData.TotalSales)));
		}

		var monthPrior = month == 1 ? 12 : month - 1;
		var monthAfter = month == 12 ? 1 : month + 1;

		AdaptiveCard card = new AdaptiveCard ()
		{
			Body =
			{
				new AdaptiveContainer()
				{
					Items =
					{
						new AdaptiveTextBlock()
						{
							Text = "Sales Results",
							Size = AdaptiveTextSize.ExtraLarge,
							Weight = AdaptiveTextWeight.Bolder
						},
						new AdaptiveTextBlock()
						{
							Text = $"Top {numberCustomers} customer sales totals for {getMonthName(month)}:"
						},
						new AdaptiveFactSet()
						{
							Facts = facts
						},
						new AdaptiveChoiceSetInput()
						{
							Id = "month",
							Style = AdaptiveChoiceInputStyle.Compact,
							Choices = new List<AdaptiveChoice>()
							{
								new AdaptiveChoice() { Title = $"{getMonthName(monthPrior)}", Value = $"{monthPrior}" },
								new AdaptiveChoice() { Title = $"{getMonthName(month)}", Value = $"{month}" },
								new AdaptiveChoice() { Title = $"{getMonthName(monthAfter)}", Value = $"{monthAfter}" }
							},
							Value = $"{month}"
						}
					}
				}
			}
		};

		return card;
	}

	private string getMonthName (int month)
	{
		return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName (month);
	}
}