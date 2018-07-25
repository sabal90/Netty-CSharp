using io.netty.util;
using System.Net;

namespace io.netty.channel
{
	public interface AddressedEnvelope<M, A> : ReferenceCounted
		where A : SocketAddress
	{
		/**
		 * Returns the message wrapped by this envelope message.
		 */
		M content();

		/**
		 * Returns the address of the sender of this message.
		 */
		A sender();

		/**
		 * Returns the address of the recipient of this message.
		 */
		A recipient();
	}
}
