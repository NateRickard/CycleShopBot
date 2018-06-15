using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CycleShopBot
{
	public static class CycleShopBot
	{
		[FunctionName ("CycleShopBot")]
		public static async Task<HttpResponseMessage> Run ([HttpTrigger (AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info ("C# HTTP trigger function processed a request.");

			// Initialize the azure bot
			using (BotService.Initialize ())
			{
				// Deserialize the incoming activity
				string jsonContent = await req.Content.ReadAsStringAsync ();
				var activity = JsonConvert.DeserializeObject<Activity> (jsonContent);

				// authenticate incoming request and add activity.ServiceUrl to MicrosoftAppCredentials.TrustedHostNames
				// if request is authenticated
				if (!await BotService.Authenticator.TryAuthenticateAsync (req, new [] { activity }, CancellationToken.None))
				{
					return BotAuthenticator.GenerateUnauthorizedResponse (req);
				}

				if (activity != null)
				{
					BotContext.Initialize (req, activity);

					// one of these will have an interface and process it
					switch (activity.GetActivityType ())
					{
						case ActivityTypes.Message:
							await Conversation.SendAsync (activity, () => new CycleShopLuisDialog ());
							break;
						case ActivityTypes.ConversationUpdate:
							var client = new ConnectorClient (new Uri (activity.ServiceUrl));
							IConversationUpdateActivity update = activity;

							if (update.MembersAdded.Any ())
							{
								var newMembers = update.MembersAdded?.Where (t => t.Id != activity.Recipient.Id);

								if (newMembers.Any ())
								{
									var reply = activity.CreateReply ();
									reply.Text = "Welcome";

									await client.Conversations.ReplyToActivityAsync (reply);
								}

								//foreach (var newMember in newMembers)
								//{
								//  reply.Text = "Welcome";

								//  if (!string.IsNullOrEmpty (newMember.Name))
								//  {
								//      reply.Text += $" {newMember.Name}";
								//  }

								//  reply.Text += "!";

								//  await client.Conversations.ReplyToActivityAsync (reply);
								//}
							}
							break;
						case ActivityTypes.ContactRelationUpdate:
						case ActivityTypes.Typing:
						case ActivityTypes.DeleteUserData:
						case ActivityTypes.Ping:
						default:
							log.Error ($"Unknown activity type ignored: {activity.GetActivityType ()}");
							break;
					}
				}

				return req.CreateResponse (HttpStatusCode.Accepted);
			}
		}
	}
}