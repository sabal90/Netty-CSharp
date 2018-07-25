
namespace io.netty.util.concurrent
{
	public interface EventExecutorGroup : Executor
	{
		void shutdownGracefully();
		void shutdownGracefully(long quietPeriod, long timeout, TimeUnit unit);
		EventExecutor next();
	}
}
