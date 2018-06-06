using System;
using System.Text;

public class Command
{
	public string Label { get; protected set; }

	public BotAction Action { get; set; }

	public bool IsCommandLabel (string label)
	{
		return this.Label == label;
	}

	public static Command<TIn, TOut> Define<TIn, TOut> (string label, BotAction action, Func<TIn, TOut> dataFactory)
	{
		return new Command<TIn, TOut>
		(
			label,
			action,
			dataFactory
		);
	}
}

[Serializable]
public class Command<TIn, TOut> : Command
{
	public Func<TIn, TOut> DataFactory { get; protected set; }

	public Command (string label, BotAction action, Func<TIn, TOut> dataFactory)
	{
		Label = label;
		Action = action;
		DataFactory = dataFactory;
	}

	//public static Command Define (string label)
	//{
	//	return new Command
	//	{
	//		Label = label
	//	};
	//}


	public Command<TIn, TOut> WithAction (BotAction action, Func<TIn, TOut> dataFactory)
	{
		this.Action = action;
		this.DataFactory = dataFactory;

		return this;
	}

	

	public string RenderActionEvent (TIn arg)
	{
		if (DataFactory != null)
		{
			var data = DataFactory (arg);

			return Action.RenderActionEvent (data);
		}

		return Action.EventTemplate; // no data members needed
	}
}

[Serializable]
public class BotAction
{
	public string Type { get; set; }

	public string EventTemplate { get; private set; }

	private BotAction ()
	{
	}

	/// <summary>
	/// Defines a BotAction with the given event type and data members. 
	/// This will construct the event JSON template containing the type and data members.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="dataMembers"></param>
	/// <returns></returns>
	public static BotAction Define (string type, params string [] dataMembers)
	{
		var builder = new StringBuilder ($"{{{{ \"Type\": \"{type}\"");

		if (dataMembers != null && dataMembers.Length > 0)
		{
			for (var i = 0; i < dataMembers.Length; i++)
			{
				builder.Append ($", \"{dataMembers [i]}\": {{0}}");
			}
		}

		builder.Append (" }}");

		return new BotAction
		{
			Type = type,
			EventTemplate = builder.ToString ()
		};
	}

	public bool IsInstance (string type)
	{
		return Type == type;
	}

	/// <summary>
	/// Renders the action event JSON with the given dataMembers used to format the event template.
	/// </summary>
	/// <param name="dataMembers"></param>
	/// <returns></returns>
	public string RenderActionEvent (params object [] dataMembers)
	{
		var formattedMembers = dataMembers.Select (d => d is string ? string.Format ("\"{0}\"", d) : d).ToArray ();

		return string.Format (EventTemplate, formattedMembers);
	}
}