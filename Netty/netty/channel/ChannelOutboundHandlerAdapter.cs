using System;
using System.Net;

namespace io.netty.channel
{
	public class ChannelOutboundHandlerAdapter : ChannelHandlerAdapter, ChannelOutboundHandler
	{
		public virtual void connect(ChannelHandlerContext ctx, SocketAddress remoteAddress, SocketAddress localAddress)
		{
			ctx.connect(remoteAddress, localAddress);
		}

		public virtual void write(ChannelHandlerContext ctx, Object msg)
		{
			ctx.write(msg);
		}

		public virtual void disconnect(ChannelHandlerContext ctx)
		{
			ctx.disconnect();
		}

		public virtual void close(ChannelHandlerContext ctx)
		{
			ctx.close();
		}

		public virtual void bind(ChannelHandlerContext ctx, SocketAddress localAddress)
		{
			ctx.bind(localAddress);
		}

		public virtual void bind(ChannelHandlerContext ctx, String deviceName)
		{
			ctx.bind(deviceName);
		}
	}
}
