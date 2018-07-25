using System;

namespace io.netty.channel
{
	public interface ChannelInboundHandler : ChannelHandler
	{
		void channelRegistered(ChannelHandlerContext ctx);
		void channelActive(ChannelHandlerContext ctx);
		void channelRead(ChannelHandlerContext ctx, Object msg);
		void channelReadComplete(ChannelHandlerContext ctx);
		void channelInactive(ChannelHandlerContext ctx);
		void channelUnregistered(ChannelHandlerContext ctx);
		void userEventTriggered(ChannelHandlerContext ctx, Object _event);
		void channelWritabilityChanged(ChannelHandlerContext ctx);
//		void exceptionCaught(ChannelHandlerContext ctx, Exception cause);
	}
}
