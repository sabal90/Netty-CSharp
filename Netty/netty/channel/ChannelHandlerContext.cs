
using io.netty.util.concurrent;
namespace io.netty.channel
{
	public interface ChannelHandlerContext : ChannelInboundInvoker, ChannelOutboundInvoker
	{
		Channel channel();
		ChannelHandler handler();
		ChannelPipeline pipeline();
		EventExecutor executor();
		bool isRemoved();
	}
}
