using System;

namespace io.netty.util.concurrent
{
	public abstract class AbstractEventExecutorGroup : EventExecutorGroup
	{
		public void shutdownGracefully()
		{
			shutdownGracefully(AbstractEventExecutor.DEFAULT_SHUTDOWN_QUIET_PERIOD, AbstractEventExecutor.DEFAULT_SHUTDOWN_TIMEOUT, TimeUnit.SECONDS);
		}

		public void execute(Action command)
		{
			next().execute(command);
		}

		public abstract EventExecutor next();
		public abstract void shutdownGracefully(long quietPeriod, long timeout, TimeUnit unit);
	}
}
