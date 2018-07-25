using NLog;
using System;
using System.Threading;

namespace io.netty.util.concurrent
{
	public abstract class AbstractEventExecutor : EventExecutor
	{
		public static long DEFAULT_SHUTDOWN_QUIET_PERIOD = 2;
		public static long DEFAULT_SHUTDOWN_TIMEOUT = 15;
		private EventExecutorGroup parent;
		private static Logger logger = LogManager.GetCurrentClassLogger();

		protected AbstractEventExecutor(EventExecutorGroup parent)
		{
			this.parent = parent;
		}

		public virtual EventExecutor next()
		{
			return this;
		}

		public void shutdownGracefully()
		{
			shutdownGracefully(DEFAULT_SHUTDOWN_QUIET_PERIOD, DEFAULT_SHUTDOWN_TIMEOUT, TimeUnit.SECONDS);
		}

		public bool inEventLoop()
		{
			return inEventLoop(Thread.CurrentThread);
		}

		protected static void safeExecute(Action task)
		{
			try
			{
				task();
			}
			catch (Exception t)
			{
				logger.Warn("A task raised an exception. Task : " + t.Message);
			}
		}

		public abstract bool inEventLoop(Thread thread);
		public abstract void execute(Action action);
		public abstract void shutdownGracefully(long quietPeriod, long timeout, TimeUnit unit);
	}
}
