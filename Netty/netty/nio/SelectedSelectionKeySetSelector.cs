
namespace io.netty.nio
{
	public class SelectedSelectionKeySetSelector : Selector
	{
		private SelectedSelectionKeySet selectionKeys;
		private Selector _delegate;

		public SelectedSelectionKeySetSelector(Selector _delegate, SelectedSelectionKeySet selectionKeys)
		{
			this._delegate = _delegate;
			this.selectionKeys = selectionKeys;
		}
	}
}
