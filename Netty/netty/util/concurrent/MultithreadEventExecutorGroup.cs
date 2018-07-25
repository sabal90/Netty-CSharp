using System;
using System.Threading;

namespace io.netty.util.concurrent
{
	public abstract class MultithreadEventExecutorGroup : AbstractEventExecutorGroup
	{
		private EventExecutor[] children;
		private EventExecutorChooser chooser;

		protected MultithreadEventExecutorGroup(int nThreads, Executor executor, EventExecutorChooserFactory chooserFactory, params Object[] args)
		{
			if (nThreads <= 0)
			{
				throw new ArgumentException(String.Format("nThreads: {0} (expected: > 0)", nThreads));
			}

			if (executor == null)
			{
				executor = new ThreadPerTaskExecutor(newDefaultThreadFactory());
			}

			children = new EventExecutor[nThreads];

			for (int i = 0; i < nThreads; i++)
			{
				bool success = false;

				try
				{
					children[i] = newChild(executor, args);
					success = true;
				}
				catch (Exception e)
				{
					// TODO: Think about if this is a good exception type
					throw new ArgumentException("failed to create a child event loop", e);
				}
				finally
				{
					if (!success)
					{
						for (int j = 0; j < i; j++)
						{
							children[j].shutdownGracefully();
						}

						for (int j = 0; j < i; j++)
						{
							EventExecutor e = children[j];

							try
							{
// 								while (!e.isTerminated())
// 								{
// 									e.awaitTermination(Integer.MAX_VALUE, TimeUnit.SECONDS);
// 								}
							}
							catch (ThreadInterruptedException/* interrupted*/)
							{
								// Let the caller handle the interruption.
//								Thread.currentThread().interrupt();
								break;
							}
						}
					}
				}
			}

			chooser = chooserFactory.newChooser(children);
		}

		public override EventExecutor next()
		{
			return chooser.next();
		}

		protected ThreadFactory newDefaultThreadFactory()
		{
			return new DefaultThreadFactory();
		}

		public override void shutdownGracefully(long quietPeriod, long timeout, TimeUnit unit)
		{
			foreach (EventExecutor l in children)
			{
				l.shutdownGracefully(quietPeriod, timeout, unit);
			}
//			return terminationFuture();
		}

		protected abstract EventExecutor newChild(Executor executor, params Object[] args);
	}
}
