using io.netty.util.concurrent;
using System;
using System.Collections.Concurrent;

namespace io.netty.channel
{
	public abstract class SingleThreadEventLoop : SingleThreadEventExecutor, EventLoop
	{
		protected static int DEFAULT_MAX_PENDING_TASKS = Math.Max(16, Int32.MaxValue);
		private BlockingCollection<Action> tailTasks;

		protected SingleThreadEventLoop(EventLoopGroup parent, Executor executor, bool addTaskWakesUp, int maxPendingTasks)
			: base(parent, executor, addTaskWakesUp, maxPendingTasks)
		{
			tailTasks = newTaskQueue(maxPendingTasks);
		}

		public override EventExecutor next()
		{
			return (EventExecutor)base.next();
		}

		public void register(Channel channel)
		{
			channel.getUnsafe().register(this);
		}

		public override void shutdownGracefully(long quietPeriod, long timeout, TimeUnit unit)
		{
			if (quietPeriod < 0)
			{
				throw new ArgumentException("quietPeriod: " + quietPeriod + " (expected >= 0)");
			}

			if (timeout < quietPeriod)
			{
				throw new ArgumentException("timeout: " + timeout + " (expected >= quietPeriod (" + quietPeriod + "))");
			}

			isTask = false;
// 			if (isShuttingDown())
// 			{
// 				return terminationFuture();
// 			}
// 
// 			bool inEventLoop = inEventLoop();
// 			bool wakeup;
// 			int oldState;
// 
// 			for (; ; )
// 			{
// 				if (isShuttingDown())
// 				{
// 					return terminationFuture();
// 				}
// 				int newState;
// 				wakeup = true;
// 				oldState = state;
// 
// 				if (inEventLoop)
// 				{
// 					newState = ST_SHUTTING_DOWN;
// 				}
// 				else
// 				{
// 					switch (oldState)
// 					{
// 						case ST_NOT_STARTED:
// 						case ST_STARTED:
// 							newState = ST_SHUTTING_DOWN;
// 							break;
// 						default:
// 							newState = oldState;
// 							wakeup = false;
// 					}
// 				}
// 
// 				if (STATE_UPDATER.compareAndSet(this, oldState, newState))
// 				{
// 					break;
// 				}
// 			}
// 
// 			gracefulShutdownQuietPeriod = unit.toNanos(quietPeriod);
// 			gracefulShutdownTimeout = unit.toNanos(timeout);
// 
// 			if (oldState == ST_NOT_STARTED)
// 			{
// 				doStartThread();
// 			}
// 
// 			if (wakeup)
// 			{
// 				wakeup(inEventLoop);
// 			}
// 
// 			return terminationFuture();
		}
	}
}
