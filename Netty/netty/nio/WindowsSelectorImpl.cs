using System;
using System.Collections.Generic;

namespace io.netty.nio
{
	public class WindowsSelectorImpl : SelectorImpl
	{
		private const int INIT_CAP = 8;
		private const int MAX_SELECTABLE_FDS = 1024;
		private SelectionKeyImpl[] channelArray = new SelectionKeyImpl[INIT_CAP];
		private int totalChannels = 1;
		private Object closeLock = new Object();
		// Lock for interrupt triggering and clearing
		private Object interruptLock = new Object();

		// Maps file descriptors to their indices in  pollArray
		private class FdMap : Dictionary<int, MapEntry>
		{
			public static long serialVersionUID = 0L;
			private MapEntry get(int desc)
			{
				MapEntry value;
				TryGetValue(desc, out value);
				return value;
			}

			private MapEntry put(SelectionKeyImpl ski)
			{
				MapEntry mapEntry = new MapEntry(ski);
				Add(((SelChImpl)ski.channel()).getFDVal(), mapEntry);
				return mapEntry;
			}

			private MapEntry remove(SelectionKeyImpl ski)
			{
				int fd = ((SelChImpl)ski.channel()).getFDVal();
				MapEntry x = get(fd);

				if ((x != null) && (x.ski.channel() == ski.channel()))
				{
					Remove(fd);
					return x;
				}

				return null;
			}
		}

		// class for fdMap entries
		private class MapEntry
		{
			public SelectionKeyImpl ski;
			public long updateCount = 0;
			public long clearedCount = 0;

			public MapEntry(SelectionKeyImpl ski)
			{
				this.ski = ski;
			}
		}

		public WindowsSelectorImpl(SelectorProvider sp) : base(sp) { }

		public SelectionKeyImpl[] getChannels()
		{
			return channelArray;
		}

		protected void implClose()
		{
			lock (closeLock)
			{
				if (channelArray != null)
				{
// 					if (pollWrapper != null)
// 					{
// 						// prevent further wakeup
// 						lock (interruptLock)
// 						{
// 							interruptTriggered = true;
// 						}
// 
// 						wakeupPipe.sink().close();
// 						wakeupPipe.source().close();
// 
// 						for (int i = 1; i < totalChannels; i++) // Deregister channels
// 						{
// 							if (i % MAX_SELECTABLE_FDS != 0) // skip wakeupEvent
// 							{
// 								deregister(channelArray[i]);
// 								SelectableChannel selch = channelArray[i].channel();
// 
// 								if (!selch.isOpen() && !selch.isRegistered())
// 									((SelChImpl)selch).kill();
// 							}
// 						}
// 
// 						pollWrapper.free();
// 						pollWrapper = null;
// 						selectedKeys = null;
// 						channelArray = null;
// 
// 						// Make all remaining helper threads exit
// 						foreach (SelectThread t in threads)
// 							t.makeZombie();
// 
// 						startLock.startThreads();
// 					}
				}
			}
		}

		protected override void implRegister(SelectionKeyImpl ski)
		{
			lock (closeLock)
			{
// 				if (pollWrapper == null)
// 					throw new ClosedSelectorException();

				growIfNeeded();
				channelArray[totalChannels] = ski;
				ski.setIndex(totalChannels);
//				fdMap.put(ski);
				keys.Add(ski);
//				pollWrapper.addEntry(totalChannels, ski);
				totalChannels++;
			}
		}

		private void growIfNeeded()
		{
			if (channelArray.Length == totalChannels)
			{
				int newSize = totalChannels * 2; // Make a larger array
				SelectionKeyImpl[] temp = new SelectionKeyImpl[newSize];
				Array.Copy(channelArray, 1, temp, 1, totalChannels - 1);
				channelArray = temp;
//				pollWrapper.grow(newSize);
			}

			if (totalChannels % MAX_SELECTABLE_FDS == 0)
			{ // more threads needed
//				pollWrapper.addWakeupSocket(wakeupSourceFd, totalChannels);
				totalChannels++;
//				threadsCount++;
			}
		}

		protected override void implDereg(SelectionKeyImpl ski)
		{
			int i = ski.getIndex();

			if (i < 0)
				throw new Exception();

			lock (closeLock)
			{
				if (i != totalChannels - 1)
				{
					// Copy end one over it
					SelectionKeyImpl endChannel = channelArray[totalChannels - 1];
					channelArray[i] = endChannel;
					endChannel.setIndex(i);
//					pollWrapper.replaceEntry(pollWrapper, totalChannels - 1, pollWrapper, i);
				}
				ski.setIndex(-1);
			}
			channelArray[totalChannels - 1] = null;
			totalChannels--;
			if (totalChannels != 1 && totalChannels % MAX_SELECTABLE_FDS == 1)
			{
				totalChannels--;
//				threadsCount--; // The last thread has become redundant.
			}
//			fdMap.remove(ski); // Remove the key from fdMap, keys and selectedKeys
			keys.Remove(ski);
			selectedKeys.Remove(ski);
			deregister(ski);
			SelectableChannel selch = ski.channel();
// 			if (!selch.isOpen() && !selch.isRegistered())
// 				((SelChImpl)selch).kill();
		}

		public void deRegister(SelectionKeyImpl ski)
		{
			implDereg(ski);
		}
	}
}
