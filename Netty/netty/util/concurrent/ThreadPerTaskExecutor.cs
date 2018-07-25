using System;

namespace io.netty.util.concurrent
{
	public class ThreadPerTaskExecutor : Executor
	{
		private ThreadFactory threadFactory;

		public ThreadPerTaskExecutor(ThreadFactory threadFactory)
		{
			if (threadFactory == null)
			{
				throw new NullReferenceException("threadFactory");
			}

			this.threadFactory = threadFactory;
		}

		public void execute(Action command)
		{
			threadFactory.newThread(command).Start();
		}
	}
}
