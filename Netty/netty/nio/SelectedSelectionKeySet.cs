using System;
using System.Collections.Generic;

namespace io.netty.nio
{
	public class SelectedSelectionKeySet : HashSet<SelectionKey>
	{
		SelectionKey[] keys;
		int _size;

		public SelectedSelectionKeySet()
		{
			keys = new SelectionKey[1024];
		}

		public new bool Add(SelectionKey o)
		{
			if (o == null)
			{
				return false;
			}

			keys[_size++] = o;

			if (_size == keys.Length)
			{
				increaseCapacity();
			}

			return true;
		}

		public int size()
		{
			return _size;
		}

		public bool remove(Object o)
		{
			return false;
		}

		public bool contains(Object o)
		{
			return false;
		}

		void reset()
		{
			reset(0);
		}

		void reset(int start)
		{
			for (int i = start; i < _size; i++)
				keys[i] = null;

			_size = 0;
		}

		private void increaseCapacity()
		{
			SelectionKey[] newKeys = new SelectionKey[keys.Length << 1];
			Array.Copy(keys, 0, newKeys, 0, _size);
			keys = newKeys;
		}
	}
}
