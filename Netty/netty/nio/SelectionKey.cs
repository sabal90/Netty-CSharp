using System;
using System.Threading;

namespace io.netty.nio
{
	public abstract class SelectionKey
	{
		protected SelectionKey() { }

		// -- Channel and selector operations --

		/**
		 * Returns the channel for which this key was created.  This method will
		 * continue to return the channel even after the key is cancelled.
		 *
		 * @return  This key's channel
		 */
		public abstract SelectableChannel channel();

		/**
		 * Returns the selector for which this key was created.  This method will
		 * continue to return the selector even after the key is cancelled.
		 *
		 * @return  This key's selector
		 */
		public abstract Selector selector();
		private Object _attachment = null;

		public Object attach(Object ob)
		{
			return Interlocked.Exchange(ref this._attachment, (Object)ob);
		}

		/**
		 * Retrieves the current attachment.
		 *
		 * @return  The object currently attached to this key,
		 *          or <tt>null</tt> if there is no attachment
		 */
		public Object attachment()
		{
			return _attachment;
		}
	}
}
