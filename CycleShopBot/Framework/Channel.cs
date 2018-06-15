using System.Collections.Generic;
using System.Linq;

namespace CycleShopBot.Framework
{
	public sealed class Channel
	{
		private Channel (ChannelType channelType, string channelId)
		{
			ChannelType = channelType;
			ChannelId = channelId;

			DefinedChannels.Add (this);
		}

		static readonly List<Channel> DefinedChannels = new List<Channel> ();

		public ChannelType ChannelType { get; private set; }

		public string ChannelId { get; private set; }

		public static Channel FromId (string channelId)
		{
			return DefinedChannels.FirstOrDefault (c => c.ChannelId == channelId) ?? Unknown;
		}

		static Channel From (ChannelType channelType, string channelId) => new Channel (channelType, channelId);

		public static Channel Unknown = From (ChannelType.None, "Unknown");
		public static Channel Emulator = From (ChannelType.Emulator, "emulator");
		public static Channel WebChat = From (ChannelType.WebChat, "webchat");
		public static Channel Teams = From (ChannelType.Teams, "msteams");
		public static Channel Skype = From (ChannelType.Skype, "skype");
		public static Channel DirectLine = From (ChannelType.DirectLine, "directline");
		// add more as needed... don't know the ids of all channels and only specifically looking for these currently

		readonly Dictionary<string, CardSupport?> ChannelCardSupport = new Dictionary<string, CardSupport?> ();

		public void RegisterCardSupport<TCard> (TCard cardType, CardSupport cardSupport)
		{
			ChannelCardSupport [cardType.ToString ()] = cardSupport;
		}

		public CardSupport GetCardSupport<TCard> (TCard cardType)
		{
			ChannelCardSupport.TryGetValue (cardType.ToString (), out CardSupport? cardSupport);

			return cardSupport ?? CardSupport.Unknown;
		}
	}
}