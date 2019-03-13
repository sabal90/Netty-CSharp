using io.netty.channel;
using io.netty.util.concurrent;
using System;
using System.Diagnostics;
using System.Threading;

namespace io.netty.handler.timeout
{
	public class IdleStateHandler : ChannelDuplexHandler
	{
		static long MIN_TIMEOUT_NANOS = TimeUnit.MILLISECONDS.toNanos(1);

		private bool observeOutput;
		private long readerIdleTimeNanos;
		private long writerIdleTimeNanos;
		private long allIdleTimeNanos;

		private Timer readerTimer;
		private long lastReadTime;
		private bool firstReaderIdleEvent = true;

		private Timer writerTimer;
		private long lastWriteTime;
		private bool firstWriterIdleEvent = true;

		private Timer allTimer;
		private bool firstAllIdleEvent = true;

		private byte state; // 0 - none, 1 - initialized, 2 - destroyed
		private bool reading;

// 		private long lastChangeCheckTimeStamp;
// 		private int lastMessageHashCode;
// 		private long lastPendingWriteBytes;

		public IdleStateHandler(int readerIdleTimeSeconds, int writerIdleTimeSeconds, int allIdleTimeSeconds)
			: this(readerIdleTimeSeconds, writerIdleTimeSeconds, allIdleTimeSeconds, TimeUnit.SECONDS)
		{
		}

		public IdleStateHandler(long readerIdleTime, long writerIdleTime, long allIdleTime, TimeUnit unit)
			: this(false, readerIdleTime, writerIdleTime, allIdleTime, unit)
		{
		}

		public IdleStateHandler(bool observeOutput, long readerIdleTime, long writerIdleTime, long allIdleTime, TimeUnit unit)
		{
			this.observeOutput = observeOutput;

			if (readerIdleTime <= 0)
			{
				readerIdleTimeNanos = 0;
			}
			else
			{
				readerIdleTimeNanos = Math.Max(unit.toNanos(readerIdleTime), MIN_TIMEOUT_NANOS);
			}

			if (writerIdleTime <= 0)
			{
				writerIdleTimeNanos = 0;
			}
			else
			{
				writerIdleTimeNanos = Math.Max(unit.toNanos(writerIdleTime), MIN_TIMEOUT_NANOS);
			}

			if (allIdleTime <= 0)
			{
				allIdleTimeNanos = 0;
			}
			else
			{
				allIdleTimeNanos = Math.Max(unit.toNanos(allIdleTime), MIN_TIMEOUT_NANOS);
			}
		}

		public long getReaderIdleTimeInMillis()
		{
			return TimeUnit.NANOSECONDS.toMillis(readerIdleTimeNanos);
		}

		public long getWriterIdleTimeInMillis()
		{
			return TimeUnit.NANOSECONDS.toMillis(writerIdleTimeNanos);
		}

		public long getAllIdleTimeInMillis()
		{
			return TimeUnit.NANOSECONDS.toMillis(allIdleTimeNanos);
		}

		public override void handlerAdded(ChannelHandlerContext ctx)
		{
			if (ctx.channel().isActive() && ctx.channel().isRegistered())
			{
				// channelActive() event has been fired already, which means this.channelActive() will
				// not be invoked. We have to initialize here instead.
				initialize(ctx);
			}
			else
			{
				// channelActive() event has not been fired yet.  this.channelActive() will be invoked
				// and initialization will occur there.
			}
		}

		public override void handlerRemoved(ChannelHandlerContext ctx)
		{
			destroy();
		}

		public override void channelRegistered(ChannelHandlerContext ctx)
		{
			// Initialize early if channel is active already.
			if (ctx.channel().isActive())
			{
				initialize(ctx);
			}
			base.channelRegistered(ctx);
		}

		public override void channelActive(ChannelHandlerContext ctx)
		{
			// This method will be invoked only if this handler was added
			// before channelActive() event is fired.  If a user adds this handler
			// after the channelActive() event, initialize() will be called by beforeAdd().
			initialize(ctx);
			base.channelActive(ctx);
		}

		public override void channelInactive(ChannelHandlerContext ctx)
		{
			destroy();
			base.channelInactive(ctx);
		}

		public override void channelRead(ChannelHandlerContext ctx, Object msg)
		{
			if (readerIdleTimeNanos > 0 || allIdleTimeNanos > 0)
			{
				reading = true;
				firstReaderIdleEvent = firstAllIdleEvent = true;
			}
			ctx.fireChannelRead(msg);
		}

		public override void channelReadComplete(ChannelHandlerContext ctx)
		{
			if ((readerIdleTimeNanos > 0 || allIdleTimeNanos > 0) && reading)
			{
				lastReadTime = ticksInNanos();
				reading = false;
			}
			ctx.fireChannelReadComplete();
		}

		public override void write(ChannelHandlerContext ctx, Object msg)
		{
			// Allow writing with void promise if handler is only configured for read timeout events.
			ctx.write(msg);

			if (writerIdleTimeNanos > 0 || allIdleTimeNanos > 0)
			{
				lastWriteTime = ticksInNanos();
				firstWriterIdleEvent = firstAllIdleEvent = true;
//				ctx.write(msg).addListener(writeListener);
			}
		}

		private void initialize(ChannelHandlerContext ctx)
		{
			// Avoid the case where destroy() is called before scheduling timeouts.
			// See: https://github.com/netty/netty/issues/143
			switch (state)
			{
				case 1:
					break;
				case 2:
					return;
			}

			state = 1;
			initOutputChanged(ctx);

			lastReadTime = lastWriteTime = ticksInNanos();

			if (readerIdleTimeNanos > 0)
			{
				readerTimer = new Timer((readerCallback) =>
				{
					new ReaderIdleTimeoutTask(this, ctx).run();
				}, null, getReaderIdleTimeInMillis(), Timeout.Infinite);
			}

			if (writerIdleTimeNanos > 0)
			{
				writerTimer = new Timer(writerCallback =>
				{
					new WriterIdleTimeoutTask(this, ctx).run();
				}, null, getWriterIdleTimeInMillis(), Timeout.Infinite);
			}

			if (allIdleTimeNanos > 0)
			{
				allTimer = new Timer(allCallback =>
				{
					new AllIdleTimeoutTask(this, ctx).run();
				}, null, getAllIdleTimeInMillis(), Timeout.Infinite);
			}
		}

		long ticksInNanos()
		{
			return (long)(Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000000000.0));
		}

		/**
		 * This method is visible for testing!
		 */
// 		ScheduledFuture<?> schedule(ChannelHandlerContext ctx, Runnable task, long delay, TimeUnit unit)
// 		{
// 			return ctx.executor().schedule(task, delay, unit);
// 		}

		private void destroy()
		{
//			state = 2;

			if (readerTimer != null)
			{
				readerTimer.Dispose();
			}

			if (writerTimer != null)
			{
				writerTimer.Dispose();
			}

			if (allTimer != null)
			{
				allTimer.Dispose();
			}
		}

		protected void channelIdle(ChannelHandlerContext ctx, IdleStateEvent evt)
		{
			ctx.fireUserEventTriggered(evt);
		}

		private void initOutputChanged(ChannelHandlerContext ctx)
		{
			if (observeOutput)
			{
				Channel channel = ctx.channel();
				Unsafe _unsafe = channel.getUnsafe();
// 				ChannelOutboundBuffer buf = _unsafe.outboundBuffer();
// 
// 				if (buf != null)
// 				{
// 					lastMessageHashCode = System.identityHashCode(buf.current());
// 					lastPendingWriteBytes = buf.totalPendingWriteBytes();
// 				}
			}
		}

		protected IdleStateEvent newIdleStateEvent(IdleState state, bool first)
		{
			switch (state)
			{
				case IdleState.ALL_IDLE:
					return first ? IdleStateEvent.FIRST_ALL_IDLE_STATE_EVENT : IdleStateEvent.ALL_IDLE_STATE_EVENT;
				case IdleState.READER_IDLE:
					return first ? IdleStateEvent.FIRST_READER_IDLE_STATE_EVENT : IdleStateEvent.READER_IDLE_STATE_EVENT;
				case IdleState.WRITER_IDLE:
					return first ? IdleStateEvent.FIRST_WRITER_IDLE_STATE_EVENT : IdleStateEvent.WRITER_IDLE_STATE_EVENT;
				default:
					throw new ArgumentException("Unhandled: state=" + state + ", first=" + first);
			}
		}

		private abstract class AbstractIdleTask
		{
			private ChannelHandlerContext ctx;

			protected AbstractIdleTask(ChannelHandlerContext ctx)
			{
				this.ctx = ctx;
			}

			public void run()
			{
				if (!ctx.channel().isOpen())
				{
					return;
				}

				run(ctx);
			}

			protected abstract void run(ChannelHandlerContext ctx);
		}

		private class ReaderIdleTimeoutTask : AbstractIdleTask
		{
			private IdleStateHandler idleStateHandler;

			public ReaderIdleTimeoutTask(IdleStateHandler idleStateHandler, ChannelHandlerContext ctx) : base(ctx)
			{
				this.idleStateHandler = idleStateHandler;
			}

			protected override void run(ChannelHandlerContext ctx)
			{
				long nextDelay = idleStateHandler.readerIdleTimeNanos;

				if (!idleStateHandler.reading)
				{
					nextDelay -= idleStateHandler.ticksInNanos() - idleStateHandler.lastReadTime;
				}

				if (nextDelay <= 0)
				{
					// Reader is idle - set a new timeout and notify the callback.
					idleStateHandler.readerTimer.Change(idleStateHandler.getReaderIdleTimeInMillis(), Timeout.Infinite);

					bool first = idleStateHandler.firstReaderIdleEvent;
					idleStateHandler.firstReaderIdleEvent = false;

					try
					{
						IdleStateEvent _event = idleStateHandler.newIdleStateEvent(IdleState.READER_IDLE, first);
						idleStateHandler.channelIdle(ctx, _event);
					}
					catch (Exception t)
					{
						ctx.fireExceptionCaught(t);
					}
				}
				else
				{
					// Read occurred before the timeout - set a new timeout with shorter delay.
					idleStateHandler.readerTimer.Change(TimeUnit.NANOSECONDS.toMillis(nextDelay), Timeout.Infinite);
				}
			}
		}

		private class WriterIdleTimeoutTask : AbstractIdleTask
		{
			private IdleStateHandler idleStateHandler;

			public WriterIdleTimeoutTask(IdleStateHandler idleStateHandler, ChannelHandlerContext ctx) : base(ctx)
			{
				this.idleStateHandler = idleStateHandler;
			}

			protected override void run(ChannelHandlerContext ctx)
			{
				long lastWriteTime = idleStateHandler.lastWriteTime;
				long nextDelay = idleStateHandler.writerIdleTimeNanos - (idleStateHandler.ticksInNanos() - idleStateHandler.lastWriteTime);

				if (nextDelay <= 0)
				{
					// Writer is idle - set a new timeout and notify the callback.
					idleStateHandler.writerTimer.Change(idleStateHandler.getWriterIdleTimeInMillis(), Timeout.Infinite);

					bool first = idleStateHandler.firstWriterIdleEvent;
					idleStateHandler.firstWriterIdleEvent = false;

					try
					{
// 						if (idleStateHandler.hasOutputChanged(ctx, first))
// 						{
// 							return;
// 						}

						IdleStateEvent _event = idleStateHandler.newIdleStateEvent(IdleState.WRITER_IDLE, first);
						idleStateHandler.channelIdle(ctx, _event);
					}
					catch (Exception t)
					{
						ctx.fireExceptionCaught(t);
					}
				}
				else
				{
					// Write occurred before the timeout - set a new timeout with shorter delay.
					idleStateHandler.writerTimer.Change(TimeUnit.NANOSECONDS.toMillis(nextDelay), Timeout.Infinite);
				}
			}
		}

		private class AllIdleTimeoutTask : AbstractIdleTask
		{
			private IdleStateHandler idleStateHandler;

			public AllIdleTimeoutTask(IdleStateHandler idleStateHandler, ChannelHandlerContext ctx) : base(ctx)
			{
				this.idleStateHandler = idleStateHandler;
			}

			protected override void run(ChannelHandlerContext ctx)
			{
				long nextDelay = idleStateHandler.allIdleTimeNanos;

				if (!idleStateHandler.reading)
				{
					nextDelay -= idleStateHandler.ticksInNanos() - Math.Max(idleStateHandler.lastReadTime, idleStateHandler.lastWriteTime);
				}

				if (nextDelay <= 0)
				{
					// Both reader and writer are idle - set a new timeout and
					// notify the callback.
					idleStateHandler.allTimer.Change(idleStateHandler.getAllIdleTimeInMillis(), Timeout.Infinite);

					bool first = idleStateHandler.firstAllIdleEvent;
					idleStateHandler.firstAllIdleEvent = false;

					try
					{
// 						if (idleStateHandler.hasOutputChanged(ctx, first))
// 						{
// 							return;
// 						}

						IdleStateEvent _event = idleStateHandler.newIdleStateEvent(IdleState.ALL_IDLE, first);
						idleStateHandler.channelIdle(ctx, _event);
					}
					catch (Exception t)
					{
						ctx.fireExceptionCaught(t);
					}
				}
				else
				{
					// Either read or write occurred before the timeout - set a new
					// timeout with shorter delay.
					idleStateHandler.allTimer.Change(TimeUnit.NANOSECONDS.toMillis(nextDelay), Timeout.Infinite);
				}
			}
		}
	}
}
