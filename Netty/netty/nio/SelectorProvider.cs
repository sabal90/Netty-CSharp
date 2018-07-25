using io.netty.channel;
using System.Collections.Generic;

namespace io.netty.nio
{
	public class SelectorProvider
	{
		private static SelectorProvider _provider = null;
//		private List<AbstractChannel> channels = null;
		private List<AbstractSelector> selectors = null;

		private SelectorProvider()
		{
//			channels = new List<AbstractChannel>();
			selectors = new List<AbstractSelector>();
		}

		public static SelectorProvider provider()
		{
			if (_provider == null)
				_provider = new SelectorProvider();

			return _provider;
		}

		public SocketChannel openSocketChannel()
		{
			return new SocketChannelImpl(_provider);
		}

		public AbstractSelector openSelector()
		{
			AbstractSelector selector = new WindowsSelectorImpl(_provider);
			selectors.Add(selector);

			return selector;
		}

// 		public void register(AbstractChannel channel)
// 		{
// 			channels.Add(channel);
// 		}

// 		public List<AbstractChannel> getChannels()
// 		{
// 			return channels;
// 		}
	}
}
