
namespace io.netty.nio
{
	public class SelectionKeyImpl : AbstractSelectionKey
	{
		private SelChImpl _channel;
		private SelectorImpl _selector;
		private int index;

		public SelectionKeyImpl(SelChImpl ch, SelectorImpl sel)
		{
			_channel = ch;
			_selector = sel;
		}

		public override SelectableChannel channel()
		{
			return (SelectableChannel)_channel;
		}

		public override Selector selector()
		{
			return _selector;
		}

		public int getIndex()
		{
			return index;
		}

		public void setIndex(int i)
		{
			index = i;
		}
	}
}
