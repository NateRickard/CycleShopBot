using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CycleShopBot
{
	[Serializable]
	public class EmployeeList : BotActionDialog<IMessageActivity>
	{
		string selectedRegion = null;
		string selectedEmployeeID = null;
		List<EmployeeItem> employees;

		[NonSerialized]
		readonly LuisResult luisResult;

		static readonly HttpClient Client = new HttpClient ();

		public EmployeeList (LuisResult luisResult)
		{
			this.luisResult = luisResult;
		}

		public async override Task StartAsync (IDialogContext context)
		{
			var regionCode = findRegion (luisResult);

			// try to find an exact product match
			if (regionCode > 0)
			{
				await DisplayEmployeeList (context, regionCode, "None");
			}
			else
			{
				context.Call (new RegionSelectionDialog (), RegionSelected);
			}
		}

		private async Task RegionSelected (IDialogContext context, IAwaitable<string> regionResult)
		{
			try
			{
				selectedRegion = await regionResult;

				await DisplayEmployeeList (context, 0, selectedRegion);
			}
			catch (Exception ex)
			{
				context.Fail (ex);
			}
		}

		private async Task EmployeeSelected (IDialogContext context, IAwaitable<string> employeeResult)
		{
			try
			{
				selectedEmployeeID = await employeeResult;

				await DisplayEmployeeCard (context, selectedEmployeeID);
			}
			catch (Exception ex)
			{
				context.Fail (ex);
			}
		}

		/// <summary>
		/// Must have either regionCode or region supplied.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="regionCode"></param>
		/// <param name="region"></param>
		/// <returns></returns>
		private async Task DisplayEmployeeList (IDialogContext context, int regionCode, string region)
		{
			await context.SendTypingIndicator ();

			employees = await GetEmployeeListForRegion (context, regionCode, region);

			if (employees.Count > 0)
			{
				context.Call (new EmployeeSelectionDialog (employees), EmployeeSelected);
			}
		}

		private async Task DisplayEmployeeCard (IDialogContext context, string employeeId)
		{
			await context.SendTypingIndicator ();

			var employee = employees.Where (x => x.EmployeeKey == long.Parse (employeeId)).First ();

			var replyToConversation = context.MakeMessage ();
			replyToConversation.Attachments = new List<Attachment> ();

			var card = new EmployeeCard (employee);

			replyToConversation.Attachments.Add (new Attachment ()
			{
				ContentType = AdaptiveCard.ContentType,
				Content = card
			});

			await context.PostAsync (replyToConversation);

			context.Wait (MessageReceived);
		}

		private async Task<List<EmployeeItem>> GetEmployeeListForRegion (IDialogContext context, int regionCode, string region)
		{
			try
			{
				var functionUri = context.GetFunctionUrl ("SalesPeopleInRegion",
						(nameof (regionCode), regionCode),
						(nameof (region), region));

				var response = await Client.PostAsync (functionUri, null);

				using (HttpContent content = response.Content)
				{
					// read the response as a string
					var jsonRaw = await content.ReadAsStringAsync ();
					// clean up nasty formatted json
					var json = jsonRaw.Trim ('"').Replace (@"\", string.Empty);

					return JsonConvert.DeserializeObject<List<EmployeeItem>> (json, Settings);
				}
			}
			catch
			{
				await context.PostAsync ("There was an issue retrieving the data you requested.");
				await context.PostAsync ("Please try your request again.");

				throw Exceptions.DataException;
			}
		}

		static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters = {
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};

		private int findRegion (LuisResult result)
		{
			string region = "0";
			var regions = new List<string> ();
			var allRegionEntities = result.Entities?.Where (e => e.Type == LuisEntities.Regions).ToList () ?? new List<EntityRecommendation> ();

			// identify a single region entity
			foreach (var entity in allRegionEntities)
			{
				if (entity.Resolution != null && entity.Resolution.TryGetValue ("values", out object values))
				{
					var regionSynonyms = (values as List<object>).Where (v => v is string).Select (v => v as string).ToList ();

					// if we have an exact match, this is the entity we want
					if (regionSynonyms.Count == 1)
					{
						region = regionSynonyms [0];
						break;
					}
				}
			}

			return int.Parse (region);
		}
	}
}