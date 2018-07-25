using io.netty.util.concurrent;

namespace io.netty.channel
{
	public interface EventLoopGroup : EventExecutorGroup
	{
		void register(Channel channel);
	}
}
