using System;

[Serializable]
public class Command
{
	public string Label { get; protected set; }

	public BotAction Action { get; set; }

	protected Command () { }

	public bool IsCommandLabel (string label)
	{
		return this.Label == label;
	}

	internal static Command Define (string label, BotAction action)
	{
		return new Command
		{
			Label = label,
			Action = action
		};
	}

	internal static Command<TOut> Define<TOut> (string label, BotAction action, Func<TOut> dataFactory)
	{
		return new Command<TOut>
		(
			label,
			action,
			dataFactory
		);
	}

	internal static Command<TIn, TOut> Define<TIn, TOut> (string label, BotAction action, Func<TIn, TOut> dataFactory)
	{
		return new Command<TIn, TOut>
		(
			label,
			action,
			dataFactory
		);
	}

	public Command WithAction (BotAction action)
	{
		this.Action = action;

		return this;
	}

	public virtual string RenderActionEvent ()
	{
		return Action.EventTemplate; // no data members needed
	}
}

[Serializable]
public class Command<TOut> : Command
{
	public Func<TOut> DataFactory { get; protected set; }

	public Command (string label, BotAction action, Func<TOut> dataFactory)
	{
		Label = label;
		Action = action;
		DataFactory = dataFactory;
	}

	public Command<TOut> WithAction (BotAction action, Func<TOut> dataFactory)
	{
		this.Action = action;
		this.DataFactory = dataFactory;

		return this;
	}

	public override string RenderActionEvent ()
	{
		if (DataFactory != null)
		{
			var data = DataFactory ();

			return Action.RenderActionEvent (data);
		}

		return base.RenderActionEvent (); // no data members needed
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

		return base.RenderActionEvent (); // no data members needed
	}
}