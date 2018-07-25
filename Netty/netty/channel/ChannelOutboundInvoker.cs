using System;
using System.Net;

namespace io.netty.channel
{
	public interface ChannelOutboundInvoker
	{
		void connect(SocketAddress remoteAddress);
		void connect(SocketAddress remoteAddress, SocketAddress localAddress);
		void bind(SocketAddress localAddress);
		void bind(String deviceName);
		void write(Object msg);
		void disconnect();
		void close();
	}
}
