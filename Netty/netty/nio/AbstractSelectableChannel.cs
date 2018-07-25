using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace io.netty.nio
{
	public abstract class AbstractSelectableChannel : SelectableChannel
	{
		private SelectorProvider _provider;
		private SelectionKey[] keys = null;
		private int keyCount = 0;

		private Object keyLock = new Object();
		private Object regLock = new Object();

		protected AbstractSelectableChannel(SelectorProvider _provider) : base()
		{
			this._provider = _provider;
		}

		protected AbstractSelectableChannel(SelectorProvider _provider, TcpClient socket) : base(socket)
		{
			this._provider = _provider;
		}

		public SelectorProvider provider()
		{
			return _provider;
		}

		private void addKey(SelectionKey k)
		{
			Debug.Assert(Monitor.TryEnter(keyLock));

			int i = 0;
			if ((keys != null) && (keyCount < keys.Length))
			{
				// Find empty element of key array
				for (i = 0; i < keys.Length; i++)
					if (keys[i] == null)
						break;
			}
			else if (keys == null)
			{
				keys = new SelectionKey[3];
			}
			else
			{
				// Grow key array
				int n = keys.Length * 2;
				SelectionKey[] ks = new SelectionKey[n];
				for (i = 0; i < keys.Length; i++)
					ks[i] = keys[i];
				keys = ks;
				i = keyCount;
			}
			keys[i] = k;
			keyCount++;
		}

		private SelectionKey findKey(Selector sel)
		{
			lock (keyLock)
			{
				if (keys == null)
					return null;
				for (int i = 0; i < keys.Length; i++)
					if ((keys[i] != null) && (keys[i].selector() == sel))
						return keys[i];
				return null;
			}
		}

		public void removeKey(SelectionKey k)
		{
			lock (keyLock)
			{
				for (int i = 0; i < keys.Length; i++)
				{
					if (keys[i] == k)
					{
						keys[i] = null;
						keyCount--;
					}
				}

//				((AbstractSelectionKey)k).invalidate();
			}
		}

// 		private bool haveValidKeys()
// 		{
// 			lock (keyLock)
// 			{
// 				if (keyCount == 0)
// 					return false;
// 				for (int i = 0; i < keys.Length; i++)
// 				{
// 					if ((keys[i] != null) && keys[i].isValid())
// 						return true;
// 				}
// 				return false;
// 			}
// 		}


		// -- Registration --

		public override bool isRegistered()
		{
			lock (keyLock)
			{
				return keyCount != 0;
			}
		}

		public override SelectionKey keyFor(Selector sel)
		{
			return findKey(sel);
		}

		public override SelectionKey register(Selector sel, Object att)
		{
			lock (regLock)
			{
				SelectionKey k = findKey(sel);
				if (k != null)
				{
					k.attach(att);
				}
				if (k == null)
				{
					// New registration
					lock (keyLock)
					{
						// sabal continue
						k = ((AbstractSelector)sel).register(this, att);
						addKey(k);
					}
				}
				return k;
			}
		}

// 		protected void implCloseChannel()
// 		{
// 			implCloseSelectableChannel();
// 			lock (keyLock)
// 			{
// 				int count = (keys == null) ? 0 : keys.Length;
// 				for (int i = 0; i < count; i++)
// 				{
// 					SelectionKey k = keys[i];
// 					if (k != null)
// 						k.cancel();
// 				}
// 			}
// 		}
// 
// 		protected abstract void implCloseSelectableChannel();
	}
}
