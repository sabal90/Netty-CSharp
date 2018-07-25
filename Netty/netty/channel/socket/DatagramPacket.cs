using io.netty.buffer;
using io.netty.util;
using System;
using System.Net;

namespace io.netty.channel.socket
{
	public class DatagramPacket : DefaultAddressedEnvelope<ByteBuf, SocketAddress>, ByteBufHolder
	{
		/**
     * Create a new instance with the specified packet {@code data} and {@code recipient} address.
     */
		public DatagramPacket(ByteBuf data, SocketAddress recipient) : base(data, recipient) { }

		/**
		 * Create a new instance with the specified packet {@code data}, {@code recipient} address, and {@code sender}
		 * address.
		 */
		public DatagramPacket(ByteBuf data, SocketAddress recipient, SocketAddress sender) : base(data, recipient, sender) { }

		public ByteBufHolder copy()
		{
			return replace(content().copy());
		}

		public ByteBufHolder duplicate()
		{
			return replace(content().duplicate());
		}

		public ByteBufHolder retainedDuplicate()
		{
			return replace(content().retainedDuplicate());
		}

		public ByteBufHolder replace(ByteBuf content)
		{
			return new DatagramPacket(content, recipient(), sender());
		}

		public override ReferenceCounted retain()
		{
			base.retain();
			return this;
		}

		public override ReferenceCounted retain(int increment)
		{
			base.retain(increment);
			return this;
		}

		public override ReferenceCounted touch()
		{
			base.touch();
			return this;
		}

		public override ReferenceCounted touch(Object hint)
		{
			base.touch(hint);
			return this;
		}
	}
}
