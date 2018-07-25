using System;

namespace io.netty.channel
{
	public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter, ChannelInboundHandler
	{
		public virtual void channelRegistered(ChannelHandlerContext ctx)
		{
			ctx.fireChannelRegistered();
		}

		public virtual void channelActive(ChannelHandlerContext ctx)
		{
			ctx.fireChannelActive();
		}

		public virtual void channelRead(ChannelHandlerContext ctx, Object msg)
		{
			ctx.fireChannelRead(msg);
		}

		public virtual void channelReadComplete(ChannelHandlerContext ctx)
		{
			ctx.fireChannelReadComplete();
		}

		public virtual void channelInactive(ChannelHandlerContext ctx)
		{
			ctx.fireChannelInactive();
		}

		public virtual void channelUnregistered(ChannelHandlerContext ctx)
		{
			ctx.fireChannelUnregistered();
		}

		public virtual void userEventTriggered(ChannelHandlerContext ctx, Object _event)
		{
			ctx.fireUserEventTriggered(_event);
		}

		public override void exceptionCaught(ChannelHandlerContext ctx, Exception cause)
		{
			ctx.fireExceptionCaught(cause);
		}

		public virtual void channelWritabilityChanged(ChannelHandlerContext ctx)
		{
			ctx.fireChannelWritabilityChanged();
		}
	}
}
