using System;
using System.Net;

namespace io.netty.channel
{
	public interface Unsafe
	{
		void register(EventLoop eventLoo);
		void connect(SocketAddress remoteAddress, SocketAddress localAddress);
		void read();
		void write(Object msg);
		void close();
		void bind(SocketAddress _localAddress);
		void bind(String deviceName);
		void disconnect();
		void closeForcibly();
		SocketAddress localAddress();
		SocketAddress remoteAddress();
	}

	public interface Channel : ChannelOutboundInvoker
	{
		EventLoop eventLoop();
		ChannelPipeline pipeline();
		ChannelId id();
		Channel parent();
		bool isOpen();
		bool isActive();
		bool isRegistered();
		SocketAddress localAddress();
		SocketAddress remoteAddress();
		Unsafe getUnsafe();
		ChannelMetadata metadata();
	}
}
