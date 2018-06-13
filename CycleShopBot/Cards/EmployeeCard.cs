using System;
using AdaptiveCards;
using Microsoft.Bot.Connector;

namespace CycleShopBot.Cards
{
	public class EmployeeCard : AdaptiveCard
	{
		public EmployeeCard (EmployeeItem employee)
		{
			// Add text to the card.
			Body.Add (new AdaptiveTextBlock ()
			{
				Text = employee.FullName,
				Size = AdaptiveTextSize.Large,
				Weight = AdaptiveTextWeight.Bolder
			});

			// Add text to the card.
			Body.Add (new AdaptiveTextBlock ()
			{
				Text = employee.Title
			});

			Body.Add (new AdaptiveTextBlock ()
			{
				Text = $"Vacation: {employee.VacationHours} hours"
			});

			// Add buttons to the card.
			Actions.Add (new AdaptiveOpenUrlAction ()
			{
				Url = new Uri ($"mailto:{employee.EmailAddress}"),
				Title = $"Email {employee.FirstName}"
			});

			//Actions.Add(new AdaptiveOpenUrlAction()
			//{
			//    Url = new Uri("tel:{employee.Phone}"),
			//    Title = $"Call {employee.FirstName}"
			//});

			//Actions.Add(new AdaptiveOpenUrlAction()
			//{
			//    Url = new Uri($"sms:{employee.Phone}"),
			//    Title = $"Text {employee.FirstName}"
			//});
		}

		public Attachment AsAttachment ()
		{
			return new Attachment ()
			{
				ContentType = ContentType,
				Content = this
			};
		}
	}
}