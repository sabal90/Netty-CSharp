using System.Threading;

namespace io.netty.util.concurrent
{
	public interface EventExecutor : EventExecutorGroup
	{
		bool inEventLoop();
		bool inEventLoop(Thread thread);
	}
}
