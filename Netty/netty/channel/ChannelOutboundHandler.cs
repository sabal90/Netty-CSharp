using System;
using System.Net;

namespace io.netty.channel
{
	public interface ChannelOutboundHandler : ChannelHandler
	{
		void connect(ChannelHandlerContext ctx, SocketAddress remoteAddress, SocketAddress localAddress);
		void bind(ChannelHandlerContext ctx, SocketAddress localAddress);
		void bind(ChannelHandlerContext ctx, String deviceName);
		void write(ChannelHandlerContext ctx, Object msg);
		void disconnect(ChannelHandlerContext ctx);
		void close(ChannelHandlerContext ctx);
	}
}
