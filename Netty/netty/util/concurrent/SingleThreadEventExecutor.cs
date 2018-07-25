using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace io.netty.util.concurrent
{
	public abstract class SingleThreadEventExecutor : AbstractScheduledEventExecutor, OrderedEventExecutor
	{
		private static int ST_NOT_STARTED = 1;
		private static int ST_STARTED = 2;
		private static int ST_SHUTTING_DOWN = 3;
//		private static int ST_SHUTDOWN = 4;
		private static int ST_TERMINATED = 5;

		private bool addTaskWakesUp;
		private int maxPendingTasks;
		private Executor executor;
		private BlockingCollection<Action> taskQueue;

		private long lastExecutionTime;
		private int state = ST_NOT_STARTED;
		private long gracefulShutdownStartTime = 0;
		protected bool isTask = false;
		private static Logger logger = LogManager.GetCurrentClassLogger();

		protected SingleThreadEventExecutor(EventExecutorGroup parent, Executor executor, bool addTaskWakesUp, int maxPendingTasks)
			: base(parent)
		{
			this.addTaskWakesUp = addTaskWakesUp;
			this.maxPendingTasks = Math.Max(16, maxPendingTasks);

			if (executor == null)
			{
				throw new NullReferenceException("executor");
			}

			this.executor = executor;
			taskQueue = newTaskQueue(this.maxPendingTasks);
		}

		~SingleThreadEventExecutor()
		{
		}

		long nanoTime()
		{
			return (long)(Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000000000.0));
		}

		public override bool inEventLoop(Thread thread)
		{
			return isTask;
//			return thread == this.thread;
		}

		protected void interruptThread()
		{
			isTask = false;
		}

		protected void updateLastExecutionTime()
		{
			lastExecutionTime = nanoTime();
		}

		protected abstract void run();

		protected BlockingCollection<Action> newTaskQueue(int maxPendingTasks)
		{
			return new BlockingCollection<Action>(maxPendingTasks);
		}

		public override void execute(Action task)
		{
			if (task == null)
			{
				throw new NullReferenceException("task");
			}

			bool _inEventLoop = inEventLoop();

			if (_inEventLoop)
			{
				addTask(task);
			}
			else
			{
				startThread();
				addTask(task);

// 				if (isShutdown() && removeTask(task))
// 				{
// 					reject();
// 				}
			}

// 			if (!addTaskWakesUp && wakesUpForTask(task))
// 			{
// 				wakeup(inEventLoop);
// 			}
		}

		protected void addTask(Action task)
		{
			if (task == null)
			{
				throw new NullReferenceException("task");
			}

			if (!offerTask(task))
			{
//				reject(task);
			}
		}

		private bool offerTask(Action task)
		{
// 			if (isShutdown())
// 			{
// 				reject();
// 			}

			return taskQueue.TryAdd(task);
		}

		protected Action pollScheduledTask(long nanoTime)
		{
// 			BlockingCollection<Task> scheduledTaskQueue = this.scheduledTaskQueue;
// 			ScheduledFutureTask<?> scheduledTask = scheduledTaskQueue == null ? null : scheduledTaskQueue.peek();
// 
// 			if (scheduledTask == null)
// 			{
// 				return null;
// 			}
// 
// 			if (scheduledTask.deadlineNanos() <= nanoTime)
// 			{
// 				scheduledTaskQueue.remove();
// 				return scheduledTask;
// 			}

			return null;
		}

		private bool fetchFromScheduledTaskQueue()
		{
			long _nanoTime = nanoTime();
			Action scheduledTask = pollScheduledTask(_nanoTime);

			while (scheduledTask != null)
			{
				if (!taskQueue.TryAdd(scheduledTask))
				{
					// No space left in the task queue add it back to the scheduledTaskQueue so we pick it up again.
					//					scheduledTaskQueue().add((ScheduledFutureTask<?>) scheduledTask);
					return false;
				}

				scheduledTask = pollScheduledTask(_nanoTime);
			}

			return true;
		}

		protected bool runAllTasksFrom(BlockingCollection<Action> taskQueue)
		{
			Action task = pollTaskFrom(taskQueue);

			if (task == null)
			{
				return false;
			}

			while (isTask)
			{
				safeExecute(task);
				task = pollTaskFrom(taskQueue);

				if (task == null)
				{
					return true;
				}
			}

			return false;
		}

		protected static Action pollTaskFrom(BlockingCollection<Action> taskQueue)
		{
			for (; ; )
			{
				Action task;
				taskQueue.TryTake(out task);

// 				if (task == WAKEUP_TASK)
// 				{
// 					continue;
// 				}

				return task;
			}
		}

		protected bool runAllTasks()
		{
			bool fetchedAll;
			bool ranAtLeastOne = false;

			do
			{
				fetchedAll = fetchFromScheduledTaskQueue();

				if (runAllTasksFrom(taskQueue))
				{
					ranAtLeastOne = true;
				}
			}
			while (!fetchedAll); // keep on processing until we fetched all scheduled tasks.

			if (ranAtLeastOne)
			{
				lastExecutionTime = nanoTime();
			}

			afterRunningAllTasks();
			return ranAtLeastOne;
		}

		protected void afterRunningAllTasks() { }

		protected bool confirmShutdown()
		{
/*			if (!isShuttingDown())
			{
				return false;
			}

			if (!inEventLoop())
			{
				throw new IllegalStateException("must be invoked from an event loop");
			}

			cancelScheduledTasks();

			if (gracefulShutdownStartTime == 0)
			{
				gracefulShutdownStartTime = ScheduledFutureTask.nanoTime();
			}

			if (runAllTasks() || runShutdownHooks())
			{
				if (isShutdown())
				{
					// Executor shut down - no new tasks anymore.
					return true;
				}

				// There were tasks in the queue. Wait a little bit more until no tasks are queued for the quiet period or
				// terminate if the quiet period is 0.
				// See https://github.com/netty/netty/issues/4241
				if (gracefulShutdownQuietPeriod == 0)
				{
					return true;
				}
				wakeup(true);
				return false;
			}

			long nanoTime = ScheduledFutureTask.nanoTime();

			if (isShutdown() || nanoTime - gracefulShutdownStartTime > gracefulShutdownTimeout)
			{
				return true;
			}

			if (nanoTime - lastExecutionTime <= gracefulShutdownQuietPeriod)
			{
				// Check if any tasks were added to the queue every 100ms.
				// TODO: Change the behavior of takeTask() so that it returns on timeout.
				wakeup(true);
				try
				{
					Thread.sleep(100);
				}
				catch (InterruptedException e)
				{
					// Ignore
				}

				return false;
			}

			// No tasks were added for last quiet period - hopefully safe to shut down.
			// (Hopefully because we really cannot make a guarantee that there will be no execute() calls by a user.)
*/			return true;
		}

		protected void cleanup()
		{
			isTask = false;
		}

		private void startThread()
		{
			if (state == ST_NOT_STARTED)
			{
				Interlocked.CompareExchange(ref state, ST_STARTED, ST_NOT_STARTED);
				doStartThread();
			}
		}

		private void doStartThread()
		{
			isTask = true;

			executor.execute(() =>
			{
				bool success = false;
				updateLastExecutionTime();

				try
				{
					run();
					success = true;
				}
				catch (Exception t)
				{
					logger.Warn("Unexpected exception from an event executor : " + t.Message);
				}
				finally
				{
					for (; ; )
					{
						int oldState = state;
						if (oldState >= ST_SHUTTING_DOWN || (Interlocked.CompareExchange(ref state, ST_SHUTTING_DOWN, oldState) == oldState && state == ST_SHUTTING_DOWN))
						{
							break;
						}
					}

					// Check if confirmShutdown() was called at the end of the loop.
					if (success && gracefulShutdownStartTime == 0)
					{
						logger.Warn("Buggy " + typeof(EventExecutor).Name + " implementation; " + typeof(SingleThreadEventExecutor).Name + ".confirmShutdown() must be called " + "before run() implementation terminates.");
					}

					try
					{
						// Run all remaining tasks and shutdown hooks.
						for (;;)
						{
							if (confirmShutdown())
							{
								break;
							}
						}
					}
					finally
					{
						try
						{
							cleanup();
						}
						finally
						{
							Interlocked.Exchange(ref state, ST_TERMINATED);
//							threadLock.release();

							if (taskQueue.Count > 0)
							{
								logger.Warn("An event executor terminated with " + "non-empty task queue (" + taskQueue.Count + ')');
							}

//							terminationFuture.setSuccess(null);
						}
					}
				}
			});
		}
	}
}
