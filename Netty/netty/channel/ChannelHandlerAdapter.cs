
using System;
namespace io.netty.channel
{
	public class ChannelHandlerAdapter : ChannelHandler
	{
		public virtual void handlerAdded(ChannelHandlerContext ctx) { }
		public virtual void handlerRemoved(ChannelHandlerContext ctx) { }
		public virtual void exceptionCaught(ChannelHandlerContext ctx, Exception cause)
		{
			ctx.fireExceptionCaught(cause);
		}
	}
}
