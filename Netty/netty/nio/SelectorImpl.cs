using System;
using System.Collections.Generic;

namespace io.netty.nio
{
	public abstract class SelectorImpl : AbstractSelector
	{
		protected HashSet<SelectionKey> selectedKeys;
		protected HashSet<SelectionKey> keys;

		private HashSet<SelectionKey> publicKeys;             // Immutable
		private HashSet<SelectionKey> publicSelectedKeys;     // Removal allowed, but not addition

		protected SelectorImpl(SelectorProvider provider)
			: base(provider)
		{
			keys = new HashSet<SelectionKey>();
			selectedKeys = new HashSet<SelectionKey>();
//			if (Util.atBugLevel("1.4"))
			{
				publicKeys = keys;
				publicSelectedKeys = selectedKeys;
			}
// 			else
// 			{
// 				publicKeys = Collections.unmodifiableSet(keys);
// 				publicSelectedKeys = Util.ungrowableSet(selectedKeys);
// 			}
		}
		public override SelectionKey register(AbstractSelectableChannel ch, Object attachment)
		{
			if (!(ch is SelChImpl))
				throw new IllegalSelectorException();

			SelectionKeyImpl k = new SelectionKeyImpl((SelChImpl)ch, this);
			k.attach(attachment);

			lock (publicKeys)
			{
				implRegister(k);
			}

			return k;
		}

		protected void processDeregisterQueue()
		{
			// Precondition: Synchronized on this, keys, and selectedKeys
// 			HashSet<SelectionKey> cks = cancelledKeys();
// 			lock (cks)
// 			{
// 				if (!cks.isEmpty())
// 				{
// 					Iterator<SelectionKey> i = cks.iterator();
// 					while (i.hasNext())
// 					{
// 						SelectionKeyImpl ski = (SelectionKeyImpl)i.next();
// 						try
// 						{
// 							implDereg(ski);
// 						}
// 						catch (SocketException se)
// 						{
// 							throw new IOException("Error deregistering key", se);
// 						}
// 						finally
// 						{
// 							i.remove();
// 						}
// 					}
// 				}
// 			}
		}

		protected abstract void implRegister(SelectionKeyImpl ski);
		protected abstract void implDereg(SelectionKeyImpl ski);
	}
}
