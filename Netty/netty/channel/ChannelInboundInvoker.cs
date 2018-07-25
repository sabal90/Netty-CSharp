using System;

namespace io.netty.channel
{
	public interface ChannelInboundInvoker
	{
		ChannelInboundInvoker fireChannelRegistered();
		ChannelInboundInvoker fireChannelActive();
		ChannelInboundInvoker fireChannelRead(Object msg);
		ChannelInboundInvoker fireChannelReadComplete();
		ChannelInboundInvoker fireChannelInactive();
		ChannelInboundInvoker fireChannelUnregistered();
		ChannelInboundInvoker fireUserEventTriggered(Object _event);
		ChannelInboundInvoker fireExceptionCaught(Exception cause);
		ChannelInboundInvoker fireChannelWritabilityChanged();
	}
}
