using io.netty.nio;
using io.netty.util.concurrent;
using System;
using System.Collections.Generic;
using System.Threading;

namespace io.netty.channel.nio
{
	public class NioEventLoop : SingleThreadEventLoop
	{
		private SelectorProvider provider;
		private Selector selector;
		private Selector _unwrappedSelector;
		private SelectedSelectionKeySet selectedKeys;

		public NioEventLoop(NioEventLoopGroup parent, Executor executor, SelectorProvider provider)
			: base(parent, executor, false, DEFAULT_MAX_PENDING_TASKS)
		{
			if (provider == null)
				throw new NullReferenceException("selectorProvider");

			this.provider = provider;
			SelectorTuple selectorTuple = openSelector();
			selector = selectorTuple.selector;
			_unwrappedSelector = selectorTuple.unwrappedSelector;
		}

		private class SelectorTuple
		{
			public Selector unwrappedSelector;
			public Selector selector;

			public SelectorTuple(Selector unwrappedSelector)
			{
				this.unwrappedSelector = unwrappedSelector;
				this.selector = unwrappedSelector;
			}

			public SelectorTuple(Selector unwrappedSelector, Selector selector)
			{
				this.unwrappedSelector = unwrappedSelector;
				this.selector = selector;
			}
		}

		public Selector unwrappedSelector()
		{
			return _unwrappedSelector;
		}

		private SelectorTuple openSelector()
		{
			Selector unwrappedSelector;
			unwrappedSelector = provider.openSelector();
			SelectedSelectionKeySet selectedKeySet = new SelectedSelectionKeySet();
			selectedKeys = selectedKeySet;

			return new SelectorTuple(unwrappedSelector, new SelectedSelectionKeySetSelector(unwrappedSelector, selectedKeySet));
		}

		protected override void run()
		{
			while (true)
			{
				if (isTask == true)
				{
					runAllTasks();
					Thread.Sleep(1);
				}
				else
				{
					WindowsSelectorImpl windowsSelector = (WindowsSelectorImpl)_unwrappedSelector;
					SelectionKeyImpl[] selectionKeys = windowsSelector.getChannels();

					foreach (SelectionKeyImpl selectionKey in selectionKeys)
					{
						if (selectionKey == null)
							continue;

						Object a = selectionKey.attachment();

						if (!(a is AbstractChannel))
							continue;

						AbstractChannel ch = (AbstractChannel)a;
						ch.getUnsafe().close();
						runAllTasks();
					}

					break;
				}
			}
		}
	}
}
