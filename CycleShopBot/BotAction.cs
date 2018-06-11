using System;
using System.Linq;
using System.Text;

namespace CycleShopBot
{
    [Serializable]
    public class BotAction
    {
        /// <summary>
        /// The type or name of event to be triggered and handled.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// a JSON template describing the action. This is generated based on the BotAction you define and used to post back to the bot.
        /// </summary>
        public string EventTemplate { get; private set; }

        private BotAction()
        {
        }

        /// <summary>
        /// Defines a BotAction with the given event type and data members. 
        /// This will construct the event JSON template containing the type and data members.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dataMembers"></param>
        /// <returns></returns>
        public static BotAction Define(string type, params string[] dataMembers)
        {
            var builder = new StringBuilder($"{{{{ \"Type\": \"{type}\"");

            if (dataMembers != null && dataMembers.Length > 0)
            {
                for (var i = 0; i < dataMembers.Length; i++)
                {
                    builder.Append($", \"{dataMembers[i]}\": {{0}}");
                }
            }

            builder.Append(" }}");

            return new BotAction
            {
                Type = type,
                EventTemplate = builder.ToString()
            };
        }

        /// <summary>
        /// Returns a value indicating if the Type of this BotAction matches the type passed in.
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public bool IsTriggeredBy(string eventType)
        {
            return Type == eventType;
        }

        /// <summary>
        /// Renders the JSON for the event to be triggered.
        /// </summary>
        /// <param name="dataMembers"></param>
        /// <returns></returns>
        public string RenderActionEvent(params object[] dataMembers)
        {
            var formattedMembers = dataMembers.Select(d => d is string ? string.Format("\"{0}\"", d) : d).ToArray();

            return string.Format(EventTemplate, formattedMembers);
        }

        /// <summary>
        /// Returns a value indicating if the given text appears to be an event template.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsActionEvent(string text)
        {
            return text?.StartsWith("{ \"Type\":") ?? false;
        }
    }
}