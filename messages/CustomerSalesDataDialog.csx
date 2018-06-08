﻿#load "Utils.csx"
#load "BotActionDialog.csx"
#load "DecimalConverter.csx"
#load "CustomerSales.csx"
#load "LuisEntities.csx"
#load "MonthSelectionDialog.csx"
#load "ProductSelectionDialog.csx"

using System;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

using Newtonsoft.Json;

using AdaptiveCards;

[Serializable]
public class CustomerSalesDataDialog : BotActionDialog<IMessageActivity>
{
	const int DefaultNumberCustomers = 5;
	const int TooManyProductsLimit = 10;

	string selectedProduct;
	int numberCustomers;
	int selectedMonth;
	(DateTime Min, DateTime Max, int StartIndex, int EndIndex) dateRange;

	[NonSerialized]
	LuisResult luisResult;

	static readonly Dictionary<string, CardSupport?> ChannelCardSupport = new Dictionary<string, CardSupport?> ()
	{
		{ "emulator", CardSupport.Adaptive },
		{ "webchat", CardSupport.Adaptive },
		{ "msteams", CardSupport.Adaptive },
		{ "directline", CardSupport.Adaptive },
		{ "skype", CardSupport.Adaptive }
	};

	static BotAction MonthChange = BotAction.Define ("MonthChange", "Month");
	Command<int> PrevMonth;
	Command<int> NextMonth;

	public CustomerSalesDataDialog (LuisResult luisResult)
	{
		this.luisResult = luisResult;

		PrevMonth = DefineCommand<int> ("Prev Month", MonthChange, () => selectedMonth == 1 ? 12 : selectedMonth - 1);
		NextMonth = DefineCommand<int> ("Next Month", MonthChange, () => selectedMonth == 12 ? 1 : selectedMonth + 1);
	}

	public async override Task StartAsync (IDialogContext context)
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
				await context.PostAsync ("Your request matched too many products. Please try a more specific product name.");
				context.Fail (new Exception ("Too many products matched"));
			}
		}
		else
		{
			context.Fail (new Exception ("Too many products matched"));
		}
	}

	protected async override Task ProcessPostBackAction (IDialogContext context, dynamic value)
	{
		string submitType = value.Type.ToString ();

		if (MonthChange.IsTriggeredBy (submitType))
		{
			int selectedMonth = Convert.ToInt32 (value.Month);

			await context.PostAsync ($"Ok, getting the data for {Utils.GetMonthName (selectedMonth)}");

			await ShowCustomerSalesTotals (context, new AwaitableFromItem<int> (selectedMonth));
		}
	}

	private (string SelectedProduct, List<string> Products) findProducts (LuisResult result)
	{
		string product = null;
		var products = new List<string> ();
		var allProductEntities = result.Entities?.Where (e => e.Type == LuisEntities.Products).ToList () ?? new List<EntityRecommendation> ();

		// identify a single product entity, if possible; otherwise we'll let the user select
		foreach (var entity in allProductEntities)
		{
			if (entity.Resolution != null && entity.Resolution.TryGetValue ("values", out object values))
			{
				var productSynonyms = (values as List<object>).Where (v => v is string).Select (v => v as string).ToList ();

				// if we have an exact match, this is the entity we want
				if (productSynonyms.Count == 1 && entity.Entity.ToLower ().Contains (productSynonyms [0].ToLower ())) //TODO: do we need this 2nd condition??
				{
					// in this case, the actual product name should be passed in as the synonym or they should be equal
					//	e.g. "Touring Tires" will return "Touring Tire" as the synonym, when using "Touring Tire" they will be equal
					product = productSynonyms [0];
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

		if (result.TryFindEntity (LuisEntities.BuiltIn.DateRange.Identifier, out EntityRecommendation dateRangeEntity))
		{
			dateStartIndex = dateRangeEntity.StartIndex ?? -1;
			dateEndIndex = dateRangeEntity.EndIndex ?? -1;

			if (dateRangeEntity.Resolution != null && dateRangeEntity.Resolution.TryGetValue ("values", out object values))
			{
				//get all values in this list and find the first one that's a date range.. there may be separate years represented, etc.
				var ranges = (values as List<object>).Where (v => v is Dictionary<string, object>).Select (v => v as Dictionary<string, object>);
				var range = ranges.FirstOrDefault (v => v.ContainsValue (LuisEntities.BuiltIn.DateRange.Name));

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
		var numberEntities = result.Entities?.Where (e => e.Type == LuisEntities.BuiltIn.Number).ToList () ?? new List<EntityRecommendation> ();

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
		try
		{
			selectedProduct = await productResult;

			// if the date range seems to be more than a 1 month span we need to ket the user know that's not supported
			if (dateRange.Min.Month != dateRange.Max.AddSeconds (-1).Month)
			{
				context.Call (new MonthSelectionDialog (dateRange.Min, dateRange.Max), ShowCustomerSalesTotals);
			}
			else
			{
				await ShowCustomerSalesTotals (context, new AwaitableFromItem<int> (dateRange.Min.Month));
			}
		}
		catch (Exception ex)
		{
			context.Fail (ex);
		}
	}

	private async Task ShowCustomerSalesTotals (IDialogContext context, IAwaitable<int> result)
	{
		selectedMonth = await result;

		// show typing indicator before we go get the SAP data
		await Utils.SendTypingIndicator (context);

		// get SAP data with parameterized query and return it
		var data = await GetTopCustomerSalesForProduct (context, selectedProduct, selectedMonth, numberCustomers);

		CardSupport? channelCardSupport = null;

		var attachments = new List<Attachment> ();

		ChannelCardSupport.TryGetValue (context.Activity.ChannelId, out channelCardSupport);

		switch (channelCardSupport ?? CardSupport.Thumbnail)
		{
			case CardSupport.Adaptive:

				attachments.Add (getAdaptiveSalesCard (selectedProduct, numberCustomers, selectedMonth, data));
				break;
			case CardSupport.Thumbnail:

				var cards = getThumbnailSalesCard (selectedProduct, numberCustomers, selectedMonth, data);

				foreach (var card in cards)
				{
					attachments.Add (card);
				}
				break;
		}

		// create our reply and add the sales card attachment
		var replyMessage = context.MakeMessage ();
		replyMessage.Attachments = new List<Attachment> (attachments);

		if (attachments.Count > 0)
		{
			replyMessage.AttachmentLayout = AttachmentLayoutTypes.List;
		}

		await context.PostAsync (replyMessage);

		context.Wait (MessageReceived);
	}

	private async Task<List<CustomerSales>> GetTopCustomerSalesForProduct (IDialogContext context, string product, int month, int count = 5)
	{
		using (var client = new HttpClient ())
		{
			var functionUri = Utils.GetFunctionUrl (context, "TopCustomersForProduct",
				(nameof (product), product),
				(nameof (month), month),
				(nameof (count), count));

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

	private Attachment getAdaptiveSalesCard (string product, int numberCustomers, int month, List<CustomerSales> salesData)
	{
		var facts = new List<AdaptiveFact> ();

		foreach (var customerSalesData in salesData)
		{
			facts.Add (new AdaptiveFact (customerSalesData.Customer, String.Format ("{0:C}", customerSalesData.TotalSales)));
		}

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
							Text = $"{Utils.GetMonthName(month)} - {Utils.ToTitleCase(product)}",
							Size = AdaptiveTextSize.ExtraLarge,
							Weight = AdaptiveTextWeight.Bolder
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
				Title = PrevMonth.Label,
				DataJson = PrevMonth.RenderActionEvent ()
			}
		);

		card.Actions.Add (
			new AdaptiveSubmitAction ()
			{
				Title = NextMonth.Label,
				DataJson = NextMonth.RenderActionEvent ()
			}
		);

		return new Attachment ()
		{
			ContentType = AdaptiveCard.ContentType,
			Content = card
		};
	}

	private List<Attachment> getThumbnailSalesCard (string product, int numberCustomers, int month, List<CustomerSales> salesData)
	{
		var attachments = new List<Attachment> ();

		var titleCard = new ThumbnailCard ()
		{
			Title = $"{Utils.GetMonthName (month)} - {Utils.ToTitleCase (product)}"
		};

		attachments.Add (titleCard.ToAttachment ());

		foreach (var customerSalesData in salesData)
		{
			var card = new ThumbnailCard ()
			{
				Title = $"{customerSalesData.Customer}",
				Subtitle = $"{customerSalesData.TotalSales:C}"
			};

			var attachment = card.ToAttachment ();
			attachments.Add (attachment);
		}

		var cardButtons = new List<CardAction> ();

		var prevButton = new CardAction ()
		{
			Value = PrevMonth.RenderActionEvent (),
			Type = "postBack",
			Title = PrevMonth.Label
		};

		cardButtons.Add (prevButton);

		var nextButton = new CardAction ()
		{
			Value = NextMonth.RenderActionEvent (),
			Type = "postBack",
			Title = NextMonth.Label
		};

		cardButtons.Add (nextButton);

		var buttonCard = new ThumbnailCard ()
		{
			Buttons = cardButtons
		};

		attachments.Add (buttonCard.ToAttachment ());

		return attachments;
	}

	enum CardSupport
	{
		Thumbnail,
		Adaptive
	}
}