#load "Utils.csx"
#load "DecimalConverter.csx"
#load "CustomerSales.csx"
#load "MonthSelectionDialog.csx"
#load "ProductSelectionDialog.csx"

using System;
using System.Configuration;
using System.Web;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;

using AdaptiveCards;

[Serializable]
public class CustomerSalesDataDialog : IDialog<IMessageActivity>
{
	const int DefaultNumberCustomers = 5;
	const int TooManyProductsLimit = 10;

	const string Products = "Products";
	const string DateRange = "builtin.datetimeV2.daterange";
	const string Number = "builtin.number";

	string selectedProduct;
	int numberCustomers;
	(DateTime Min, DateTime Max, int StartIndex, int EndIndex) dateRange;

	[NonSerialized]
	LuisResult luisResult;

	public CustomerSalesDataDialog (LuisResult luisResult)
	{
		this.luisResult = luisResult;
	}

	public async Task StartAsync (IDialogContext context)
	{
		var products = findProducts (luisResult);
		dateRange = getDateRanges (luisResult);
		numberCustomers = getNumberCustomers (luisResult);

		// try to find an exact product match
		if (products.SelectedProduct != null)
		{
			await context.PostAsync ($"Sure, I can help you with sales data for {Utils.ToTitleCase (products.SelectedProduct)}");

			await ProductSelected (context, new AwaitableFromItem<string> (products.SelectedProduct));
		}
		else if (products.Products.Count > 0) // if more than one is matched (e.g. "Tires"), go ahead and spin up a selection dialog
		{
			if (products.Products.Count <= TooManyProductsLimit) // more than one product entity match - let them choose!
			{
				context.Call (new ProductSelectionDialog (products.Products), ProductSelected);
			}
			else
			{
				await context.PostAsync ($"Your request matched too many products. Please try a more specific product name.");
				context.Fail (new Exception ("Too many products matched"));
			}
		}
		else
		{
			context.Fail (new Exception("Too many products matched"));
		}
	}

	protected virtual async Task MessageReceived (IDialogContext context, IAwaitable<IMessageActivity> item)
	{
		var message = await item;

		if (message.Value != null)
		{
			// Got an Action Submit!
			dynamic value = message.Value;
			string submitType = value.Type.ToString ();

			switch (submitType)
			{
				case "MonthChange":
					int selectedMonth = Convert.ToInt32 (value.Month);
					await ShowCustomerSalesTotals (context, new AwaitableFromItem<int> (selectedMonth));
					return;
			}
		}
		else //exit this dialog if we don't understand/handle what's coming in
		{
			context.Done (message);
		}
	}

	private (string SelectedProduct, List<string> Products) findProducts (LuisResult result)
	{
		string product = null;
		var products = new List<string> ();
		var allProductEntities = result.Entities?.Where (e => e.Type == Products).ToList () ?? new List<EntityRecommendation> ();

		// identify a single product entity, if possible; otherwise we'll let the user select
		foreach (var entity in allProductEntities)
		{
			if (entity.Resolution != null && entity.Resolution.TryGetValue ("values", out object values))
			{
				var productSynonyms = (values as List<object>).Where (v => v is string).Select (v => v as string).ToList ();

				// if we have an exact match, this is the entity we want
				if (productSynonyms.Count == 1 && productSynonyms [0].ToLower () == entity.Entity.ToLower ())
				{
					product = entity.Entity;
					break;
				}
				else
				{
					products.AddRange (productSynonyms.Where (p => !products.Contains (p)));
				}
			}
			else // no resolved values, try to use the 'parent' entity
			{
				products.Add (entity.Entity);
			}
		}

		return (product, products);
	}

	private (DateTime Min, DateTime Max, int StartIndex, int EndIndex) getDateRanges (LuisResult result)
	{
		DateTime now = DateTime.Now;
		// default to the current month
		DateTime dateRangeMin = new DateTime (now.Year, 1, 1);
		DateTime dateRangeMax = new DateTime (now.Year, 12, 1);
		int dateStartIndex = -1;
		int dateEndIndex = -1;

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

		return (dateRangeMin, dateRangeMax, dateStartIndex, dateEndIndex);
	}

	private int getNumberCustomers (LuisResult result)
	{
		var numberEntities = result.Entities?.Where (e => e.Type == Number).ToList () ?? new List<EntityRecommendation> ();

		while (numberEntities.Count > 0)
		{
			var numberEntity = numberEntities [0];

			// sneaky phony numbers coming back when entering dates like 1/1/2018, so we axe them here
			if (numberEntity.StartIndex >= dateRange.StartIndex && numberEntity.EndIndex <= dateRange.EndIndex)
			{
				numberEntities.Remove (numberEntity);
				continue;
			}

			return Convert.ToInt32 (numberEntity.Entity);
		}

		return DefaultNumberCustomers;
	}

	private async Task ProductSelected (IDialogContext context, IAwaitable<string> productResult)
	{
		selectedProduct = await productResult;

		//await context.PostAsync ("Getting that information now...");

		// if the date range seems to be more than a 1 month span we need to ket the user know that's not supported
		if (dateRange.Min.Month != dateRange.Max.AddSeconds(-1).Month) // 1 && (dateRange.Max.Day > 1 || dateRange.Max.TimeOfDay.TotalSeconds > 1))
		{
			context.Call (new MonthSelectionDialog (dateRange.Min, dateRange.Max), ShowCustomerSalesTotals);
		}
		else
		{
			await ShowCustomerSalesTotals (context, new AwaitableFromItem<int> (dateRange.Min.Month));
		}
	}

	private async Task ShowCustomerSalesTotals (IDialogContext context, IAwaitable<int> result)
	{
		var selectedMonth = await result;

		// show a typing indicator to let the user know we're working on it?

		// get SAP data with parameterized query and return it
		var data = await GetTopCustomerSalesForProduct (selectedProduct, selectedMonth, numberCustomers);

		await context.PostAsync ($"It looks like these are the top {numberCustomers} customers that have purchased {selectedProduct} in the month of {Utils.GetMonthName (selectedMonth)}:");

		// create our reply and add the sales card attachment
		var replyMessage = context.MakeMessage ();
		var card = getSalesCard (selectedProduct, numberCustomers, selectedMonth, data);

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

	private AdaptiveCard getSalesCard (string product, int numberCustomers, int month, List<CustomerSales> salesData)
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
							Text = $"Sales Results - {Utils.ToTitleCase(product)}",
							Size = AdaptiveTextSize.ExtraLarge,
							Weight = AdaptiveTextWeight.Bolder
						},
						new AdaptiveTextBlock()
						{
							Text = $"Top {numberCustomers} customer sales totals for {Utils.GetMonthName(month)}:"
						},
						new AdaptiveFactSet()
						{
							Facts = facts
						}
					}
				}
			}
		};

		card.Actions.Add (
			new AdaptiveSubmitAction ()
			{
				Title = Utils.GetMonthName (monthPrior),
				DataJson = $"{{ \"Type\": \"MonthChange\", \"Month\": {monthPrior} }}"
			}
		);

		card.Actions.Add (
			new AdaptiveSubmitAction ()
			{
				Title = Utils.GetMonthName (monthAfter),
				DataJson = $"{{ \"Type\": \"MonthChange\", \"Month\": {monthAfter} }}"
			}
		);

		return card;
	}
}