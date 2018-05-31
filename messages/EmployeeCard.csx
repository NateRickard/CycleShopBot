using System;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using AdaptiveCards;

public class EmployeeCard : AdaptiveCard
{
    public EmployeeCard(EmployeeItem employee)
    {
        // Add text to the card.
        this.Body.Add(new AdaptiveTextBlock()
        {
            Text = employee.FullName,
            Size = AdaptiveTextSize.Large,
            Weight = AdaptiveTextWeight.Bolder
        });

        // Add text to the card.
        this.Body.Add(new AdaptiveTextBlock()
        {
            Text = employee.Title
        });

        // Add text to the card.
        this.Body.Add(new AdaptiveTextBlock()
        {
            Text = $"Vacation: {employee.VacationHours}"
        });

        // Add buttons to the card.
        this.Actions.Add(new AdaptiveOpenUrlAction()
        {
            Url = new Uri($"mailto:{employee.EmailAddress}"),
            Title = $"Email {employee.FirstName}"
        });

        this.Actions.Add(new AdaptiveOpenUrlAction()
        {
            Url = new Uri("tel:{employee.Phone}"),
            Title = $"Call {employee.FirstName}"
        });

        this.Actions.Add(new AdaptiveOpenUrlAction()
        {
            Url = new Uri($"sms:{employee.Phone}"),
            Title = $"Text {employee.FirstName}"
        });
    }
     
} 
