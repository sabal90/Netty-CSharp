
using System;
namespace io.netty.channel
{
	public interface ChannelHandler
	{
		void handlerAdded(ChannelHandlerContext ctx);
		void handlerRemoved(ChannelHandlerContext ctx);
		void exceptionCaught(ChannelHandlerContext ctx, Exception cause);
	}
}
