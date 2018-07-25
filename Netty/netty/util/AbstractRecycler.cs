using io.netty.util.inner;
using NLog;
using System;
using System.Threading;

namespace io.netty.util
{
	public abstract class AbstractRecycler<T>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

// 		private static Handle NOOP_HANDLE = new Handle() {
// 			public void recycle(Object object) {
// 				// NOOP
// 			}
// 		};
		private static int ID_GENERATOR = Int32.MinValue;
		private static int OWN_THREAD_ID = Interlocked.Increment(ref ID_GENERATOR);
// 		private static int DEFAULT_INITIAL_MAX_CAPACITY_PER_THREAD = 32768; // Use 32k instances as default.
// 		private static int DEFAULT_MAX_CAPACITY_PER_THREAD;
// 		private static int INITIAL_CAPACITY;
// 		private static int MAX_SHARED_CAPACITY_FACTOR;
// 		private static int MAX_DELAYED_QUEUES_PER_THREAD;
// 		private static int LINK_CAPACITY;
// 		private static int RATIO;

		private int maxCapacityPerThread;
		private int maxSharedCapacityFactor;
		private int ratioMask;
		private int maxDelayedQueuesPerThread;

// 		protected AbstractRecycler() : this(DEFAULT_MAX_CAPACITY_PER_THREAD) { }
// 		protected AbstractRecycler(int maxCapacityPerThread) : this(maxCapacityPerThread, MAX_SHARED_CAPACITY_FACTOR) { }
// 		protected AbstractRecycler(int maxCapacityPerThread, int maxSharedCapacityFactor) : this(maxCapacityPerThread, maxSharedCapacityFactor, RATIO, MAX_DELAYED_QUEUES_PER_THREAD) { }

		protected AbstractRecycler(int maxCapacityPerThread, int maxSharedCapacityFactor, int ratio, int maxDelayedQueuesPerThread)
		{
			ratioMask = MathUtil.safeFindNextPositivePowerOfTwo(ratio) - 1;
			if (maxCapacityPerThread <= 0)
			{
				this.maxCapacityPerThread = 0;
				this.maxSharedCapacityFactor = 1;
				this.maxDelayedQueuesPerThread = 0;
			}
			else
			{
				this.maxCapacityPerThread = maxCapacityPerThread;
				this.maxSharedCapacityFactor = Math.Max(1, maxSharedCapacityFactor);
				this.maxDelayedQueuesPerThread = Math.Max(0, maxDelayedQueuesPerThread);
			}
		}

// 		public T get()
// 		{
// 			if (maxCapacityPerThread == 0)
// 			{
// 				return newObject((Handle<T>)NOOP_HANDLE);
// 			}
// 			Stack<T> stack = threadLocal.get();
// 			DefaultHandle<T> handle = stack.pop();
// 			if (handle == null)
// 			{
// 				handle = stack.newHandle();
// 				handle.value = newObject(handle);
// 			}
// 			return (T)handle.value;
// 		}
// 
// 		public interface Handle<T>
// 		{
// 			void recycle(T Obj);
// 		}
// 
// 		protected abstract T newObject(Handle<T> handle);
	}
}
