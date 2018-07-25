using System;

namespace io.netty.channel
{
	public interface ChannelPipeline : ChannelInboundInvoker, ChannelOutboundInvoker
	{
		ChannelPipeline addLast(String name, ChannelHandler handler);
		ChannelPipeline addLast(ChannelHandler[] handlers);
		ChannelPipeline addLast(ChannelHandler handler);
		Channel channel();
		ChannelHandlerContext context(ChannelHandler handler);
		ChannelPipeline remove(ChannelHandler handler);
	}
}
