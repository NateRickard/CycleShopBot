using CycleShopBot.Cards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CycleShopBot
{
	[Serializable]
	public class EmployeeDetailsDialog : BotActionDialog<IMessageActivity>
	{
		readonly int selectedEmployeeID = 0;

		static readonly HttpClient Client = new HttpClient ();

		static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters = {
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};

		public EmployeeDetailsDialog (LuisResult luisResult)
		{
			selectedEmployeeID = FindEmployee (luisResult);
		}

		public override async Task StartAsync (IDialogContext context)
		{
			var data = await GetEmployeeDetails (context, selectedEmployeeID);

			await DisplayEmployeeCard (context, data);
		}

		private int FindEmployee (LuisResult result)
		{
			string employeeId = "0";
			var regions = new List<string> ();
			var allEmployeeEntities = result.Entities?.Where (e => e.Type == LuisEntities.BuiltIn.Number).ToList () ?? new List<EntityRecommendation> ();

			// identify a single region entity
			foreach (var entity in allEmployeeEntities)
			{
				if (entity.Resolution != null && entity.Resolution.TryGetValue ("value", out object values))
				{
					employeeId = values.ToString ();
				}
			}

			return int.Parse (employeeId);
		}

		private async Task DisplayEmployeeCard (IDialogContext context, EmployeeItem employee)
		{
			await context.SendTypingIndicator ();

			var replyToConversation = context.MakeMessage ();
			replyToConversation.Attachments = new List<Attachment> ();

			var card = new EmployeeCard (employee);
			replyToConversation.Attachments.Add (card.AsAttachment ());

			await context.PostAsync (replyToConversation);

			context.Wait (MessageReceived);
		}

		private async Task<EmployeeItem> GetEmployeeDetails (IDialogContext context, int employeeid)
		{
			try
			{
				if (!BotContext.MockDataEnabled)
				{
					var functionUri = Utils.GetFunctionUrl ("EmployeeDetails",
						(nameof (employeeid), employeeid));

					var response = await Client.PostAsync (functionUri, null);

					using (HttpContent content = response.Content)
					{
						// read the response as a string
						var json = await content.ReadAsStringAsync ();

						var data = JsonConvert.DeserializeObject<List<EmployeeItem>> (json.CleanJson (), SerializerSettings);
						return data.FirstOrDefault ();
					}
				}
				else
				{
					return MockData.Employees.FirstOrDefault (e => e.EmployeeKey == employeeid);
				}
			}
			catch
			{
				await context.PostAsync ("There was an issue retrieving the data you requested.");
				await context.PostAsync ("Please try your request again.");

				throw Exceptions.DataException;
			}
		}
	}
}