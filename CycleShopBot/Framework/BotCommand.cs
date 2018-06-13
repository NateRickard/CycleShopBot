using System;

namespace CycleShopBot
{
	[Serializable]
	public class BotCommand
	{
		public string Label { get; protected set; }

		public BotAction Action { get; set; }

		protected BotCommand () { }

		public bool IsCommandLabel (string label)
		{
			return Label == label;
		}

		internal static BotCommand Define (string label, BotAction action)
		{
			return new BotCommand
			{
				Label = label,
				Action = action
			};
		}

		internal static BotCommand<TOut> Define<TOut> (string label, BotAction action, Func<TOut> dataFactory)
		{
			return new BotCommand<TOut>
			(
				label,
				action,
				dataFactory
			);
		}

		internal static BotCommand<TIn, TOut> Define<TIn, TOut> (string label, BotAction action, Func<TIn, TOut> dataFactory)
		{
			return new BotCommand<TIn, TOut>
			(
				label,
				action,
				dataFactory
			);
		}

		public virtual string RenderActionEvent ()
		{
			return Action.EventTemplate; // no data members needed
		}
	}

	[Serializable]
	public class BotCommand<TOut> : BotCommand
	{
		public Func<TOut> DataFactory { get; protected set; }

		public BotCommand (string label, BotAction action, Func<TOut> dataFactory)
		{
			Label = label;
			Action = action;
			DataFactory = dataFactory;
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
	public class BotCommand<TIn, TOut> : BotCommand
	{
		public Func<TIn, TOut> DataFactory { get; protected set; }

		public BotCommand (string label, BotAction action, Func<TIn, TOut> dataFactory)
		{
			Label = label;
			Action = action;
			DataFactory = dataFactory;
		}

		public BotCommand<TIn, TOut> WithAction (BotAction action, Func<TIn, TOut> dataFactory)
		{
			Action = action;
			DataFactory = dataFactory;

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
}