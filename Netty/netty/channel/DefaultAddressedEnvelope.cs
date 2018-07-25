using io.netty.util;
using io.netty.util.inner;
using System;
using System.Net;

namespace io.netty.channel
{
	public class DefaultAddressedEnvelope<M, A> : AddressedEnvelope<M, A>
		where A : SocketAddress
	{
		private M message;
		private A _sender;
		private A _recipient;

		/**
		 * Creates a new instance with the specified {@code message}, {@code recipient} address, and
		 * {@code sender} address.
		 */
		public DefaultAddressedEnvelope(M message, A recipient, A sender)
		{
			if (message == null)
			{
				throw new NullReferenceException("message");
			}

			if (recipient == null && sender == null)
			{
				throw new NullReferenceException("recipient and sender");
			}

			this.message = message;
			this._sender = sender;
			this._recipient = recipient;
		}

		/**
		 * Creates a new instance with the specified {@code message} and {@code recipient} address.
		 * The sender address becomes {@code null}.
		 */
		public DefaultAddressedEnvelope(M message, A recipient) : this(message, recipient, null) { }

		public M content()
		{
			return message;
		}

		public A sender()
		{
			return _sender;
		}

		public A recipient()
		{
			return _recipient;
		}

		public int refCnt()
		{
			if (message is ReferenceCounted)
			{
				return ((ReferenceCounted)message).refCnt();
			}
			else
			{
				return 1;
			}
		}

		public virtual ReferenceCounted retain()
		{
			ReferenceCountUtil.retain(message);
			return this;
		}

		public virtual ReferenceCounted retain(int increment)
		{
			ReferenceCountUtil.retain(message, increment);
			return this;
		}

		public bool release()
		{
			return ReferenceCountUtil.release(message);
		}

		public bool release(int decrement)
		{
			return ReferenceCountUtil.release(message, decrement);
		}

		public virtual ReferenceCounted touch()
		{
			ReferenceCountUtil.touch(message);
			return this;
		}

		public virtual ReferenceCounted touch(Object hint)
		{
			ReferenceCountUtil.touch(message, hint);
			return this;
		}

		public String toString()
		{
			if (_sender != null)
			{
				return StringUtil.simpleClassName(this) +
						'(' + _sender + " => " + _recipient + ", " + message + ')';
			}
			else
			{
				return StringUtil.simpleClassName(this) +
						"(=> " + _recipient + ", " + message + ')';
			}
		}
	}
}
