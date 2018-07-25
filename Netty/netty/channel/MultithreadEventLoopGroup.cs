using io.netty.util.concurrent;
using System;

namespace io.netty.channel
{
	public abstract class MultithreadEventLoopGroup : MultithreadEventExecutorGroup, EventLoopGroup
	{
		private static int DEFAULT_EVENT_LOOP_THREADS = getDefaultEventLoopThreads();

		protected MultithreadEventLoopGroup(int nThreads, Executor executor, params Object[] args) : base(nThreads == 0 ? DEFAULT_EVENT_LOOP_THREADS : nThreads, executor, DefaultEventExecutorChooserFactory.INSTANCE, args) { }

		private static int getDefaultEventLoopThreads()
		{
			return Math.Max(1, Environment.ProcessorCount * 2);
		}

		public override EventExecutor next()
		{
			return (EventExecutor)base.next();
		}

		public void register(Channel channel)
		{
			EventExecutor eventExecutor = next();

			if (eventExecutor is EventLoop)
				((EventLoop)eventExecutor).register(channel);
		}
	}
}
