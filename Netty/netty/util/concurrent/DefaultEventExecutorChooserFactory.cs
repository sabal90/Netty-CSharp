using System;

namespace io.netty.util.concurrent
{
	public class DefaultEventExecutorChooserFactory : EventExecutorChooserFactory
	{
		public static DefaultEventExecutorChooserFactory INSTANCE = new DefaultEventExecutorChooserFactory();

		public EventExecutorChooser newChooser(EventExecutor[] executors)
		{
			if (isPowerOfTwo(executors.Length))
			{
				return new PowerOfTwoEventExecutorChooser(executors);
			}
			else
			{
				return new GenericEventExecutorChooser(executors);
			}
		}

		private static bool isPowerOfTwo(int val)
		{
			return (val & -val) == val;
		}

		private class PowerOfTwoEventExecutorChooser : EventExecutorChooser
		{
			private int idx;
			private EventExecutor[] executors;

			public PowerOfTwoEventExecutorChooser(EventExecutor[] executors)
			{
				this.executors = executors;
			}

			public EventExecutor next()
			{
				return executors[idx++ & executors.Length - 1];
			}
		}

		private class GenericEventExecutorChooser : EventExecutorChooser
		{
			private int idx;
			private EventExecutor[] executors;

			public GenericEventExecutorChooser(EventExecutor[] executors)
			{
				this.executors = executors;
			}

			public EventExecutor next()
			{
				return executors[Math.Abs(idx++ % executors.Length)];
			}
		}
	}
}
