using io.netty.util.concurrent;

namespace io.netty.channel
{
	public interface EventLoop : OrderedEventExecutor, EventLoopGroup
	{
	}
}
