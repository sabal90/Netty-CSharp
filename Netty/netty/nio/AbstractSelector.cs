using System;

namespace io.netty.nio
{
	public abstract class AbstractSelector : Selector
	{
		private SelectorProvider provider;

		protected AbstractSelector(SelectorProvider provider)
		{
			this.provider = provider;
		}

		public abstract SelectionKey register(AbstractSelectableChannel ch, Object att);
		protected void deregister(AbstractSelectionKey key)
		{
			((AbstractSelectableChannel)key.channel()).removeKey(key);
		}
	}
}
