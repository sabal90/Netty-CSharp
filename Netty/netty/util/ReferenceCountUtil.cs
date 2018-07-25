using io.netty.util.inner;
using NLog;
using System;

namespace io.netty.util
{
	public class ReferenceCountUtil
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static T retain<T>(T msg)
		{
			if (msg is ReferenceCounted)
			{
				return (T)((ReferenceCounted)msg).retain();
			}
			return msg;
		}

		/**
		 * Try to call {@link ReferenceCounted#retain(int)} if the specified message implements {@link ReferenceCounted}.
		 * If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
		 */
		public static T retain<T>(T msg, int increment)
		{
			if (msg is ReferenceCounted)
			{
				return (T)((ReferenceCounted)msg).retain(increment);
			}
			return msg;
		}

		/**
		 * Tries to call {@link ReferenceCounted#touch()} if the specified message implements {@link ReferenceCounted}.
		 * If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
		 */
		public static T touch<T>(T msg)
		{
			if (msg is ReferenceCounted)
			{
				return (T)((ReferenceCounted)msg).touch();
			}
			return msg;
		}

		/**
		 * Tries to call {@link ReferenceCounted#touch(Object)} if the specified message implements
		 * {@link ReferenceCounted}.  If the specified message doesn't implement {@link ReferenceCounted},
		 * this method does nothing.
		 */
		public static T touch<T>(T msg, Object hint)
		{
			if (msg is ReferenceCounted)
			{
				return (T)((ReferenceCounted)msg).touch(hint);
			}
			return msg;
		}

		/**
		 * Try to call {@link ReferenceCounted#release()} if the specified message implements {@link ReferenceCounted}.
		 * If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
		 */
		public static bool release(Object msg)
		{
			if (msg is ReferenceCounted)
			{
				return ((ReferenceCounted)msg).release();
			}
			return false;
		}

		/**
		 * Try to call {@link ReferenceCounted#release(int)} if the specified message implements {@link ReferenceCounted}.
		 * If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
		 */
		public static bool release(Object msg, int decrement)
		{
			if (msg is ReferenceCounted)
			{
				return ((ReferenceCounted)msg).release(decrement);
			}
			return false;
		}

		/**
		 * Try to call {@link ReferenceCounted#release()} if the specified message implements {@link ReferenceCounted}.
		 * If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
		 * Unlike {@link #release(Object)} this method catches an exception raised by {@link ReferenceCounted#release()}
		 * and logs it, rather than rethrowing it to the caller.  It is usually recommended to use {@link #release(Object)}
		 * instead, unless you absolutely need to swallow an exception.
		 */
		public static void safeRelease(Object msg)
		{
			try
			{
				release(msg);
			}
			catch (Exception t)
			{
				logger.Warn(t, "Failed to release a message: " + msg);
			}
		}

		/**
		 * Try to call {@link ReferenceCounted#release(int)} if the specified message implements {@link ReferenceCounted}.
		 * If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
		 * Unlike {@link #release(Object)} this method catches an exception raised by {@link ReferenceCounted#release(int)}
		 * and logs it, rather than rethrowing it to the caller.  It is usually recommended to use
		 * {@link #release(Object, int)} instead, unless you absolutely need to swallow an exception.
		 */
		public static void safeRelease(Object msg, int decrement)
		{
			try
			{
				release(msg, decrement);
			}
			catch (Exception t)
			{
				if (logger.IsWarnEnabled)
				{
					logger.Warn(t, String.Format("Failed to release a message: {0} (decrement: {1})", msg, decrement));
				}
			}
		}

		/**
		 * Returns reference count of a {@link ReferenceCounted} object. If object is not type of
		 * {@link ReferenceCounted}, {@code -1} is returned.
		 */
		public static int refCnt(Object msg)
		{
			return msg is ReferenceCounted ? ((ReferenceCounted)msg).refCnt() : -1;
		}

		/**
		 * Releases the objects when the thread that called {@link #releaseLater(Object)} has been terminated.
		 */
		private class ReleasingTask
		{
			private ReferenceCounted obj;
			private int decrement;

			ReleasingTask(ReferenceCounted obj, int decrement)
			{
				this.obj = obj;
				this.decrement = decrement;
			}

			public void run()
			{
				try
				{
					if (!obj.release(decrement))
					{
						logger.Warn(String.Format("Non-zero refCnt: {0}", this));
					}
					else
					{
						logger.Debug(String.Format("Released: {0}", this));
					}
				}
				catch (Exception ex)
				{
					logger.Warn(ex, String.Format("Failed to release an object: {0}", obj));
				}
			}

			public String toString()
			{
				return StringUtil.simpleClassName(obj) + ".release(" + decrement + ") refCnt: " + obj.refCnt();
			}
		}

		private ReferenceCountUtil() { }
	}
}
