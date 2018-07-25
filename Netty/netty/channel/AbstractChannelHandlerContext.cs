using io.netty.util.concurrent;
using NLog;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace io.netty.channel
{
	public abstract class AbstractChannelHandlerContext : ChannelHandlerContext
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public AbstractChannelHandlerContext next;
		public AbstractChannelHandlerContext prev;
		private DefaultChannelPipeline _pipeline;
		private EventExecutor _executor;
		private String _name;
		private bool inbound;
		private static int INIT = 0;
		private static int ADD_PENDING = 1;
		private static int ADD_COMPLETE = 2;
		private static int REMOVE_COMPLETE = 3;
		private int handlerState = INIT;
		private bool ordered;
		private bool outbound;

		public AbstractChannelHandlerContext(DefaultChannelPipeline _pipeline, EventExecutor _executor, String _name, bool inbound, bool outbound)
		{
			this._name = _name;
			this._pipeline = _pipeline;
			this._executor = _executor;
			this.inbound = inbound;
			this.outbound = outbound;
			ordered = true;
		}

		public Channel channel()
		{
			return _pipeline.channel();
		}

		public String name()
		{
			return _name;
		}

		private void invokeExceptionCaught(Exception cause)
		{
			if (invokeHandler())
			{
				try
				{
					handler().exceptionCaught(this, cause);
				}
				catch (Exception error)
				{
					if (logger.IsDebugEnabled)
					{
						logger.Debug(cause, String.Format(
							"An exception {0}" +
							"was thrown by a user handler's exceptionCaught() " +
							"method while handling the following exception:",
							error.StackTrace));
					}
					else if (logger.IsWarnEnabled)
					{
						logger.Warn(cause, String.Format(
							"An exception '{0}' [enable DEBUG level for full stacktrace] " +
							"was thrown by a user handler's exceptionCaught() " +
							"method while handling the following exception:", error.Message));
					}
				}
			}
		}

		private static bool inExceptionCaught(Exception cause)
		{
			return cause.StackTrace.IndexOf("." + "exceptionCaught" + "(", StringComparison.Ordinal) >= 0;
		}

		private void notifyOutboundHandlerException(Exception cause)
		{
			invokeExceptionCaught(cause);
		}

		private void notifyHandlerException(Exception cause)
		{
			if (inExceptionCaught(cause))
			{
				if (logger.IsWarnEnabled)
					logger.Warn(cause, "An exception was thrown by a user handler while handling an exceptionCaught event");

				return;
			}

			invokeExceptionCaught(cause);
		}

		private AbstractChannelHandlerContext findContextInbound()
		{
			AbstractChannelHandlerContext ctx = this;

			do
			{
				ctx = ctx.next;
			}
			while (!ctx.inbound);

			return ctx;
		}

		private AbstractChannelHandlerContext findContextOutbound()
		{
			AbstractChannelHandlerContext ctx = this;

			do
			{
				ctx = ctx.prev;
			}
			while (!ctx.outbound);

			return ctx;
		}

		public static void invokeChannelRegistered(AbstractChannelHandlerContext next)
		{
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeChannelRegistered();
			}
			else
				executor.execute(() => next.invokeChannelRegistered());

		}

		public EventExecutor executor()
		{
			if (_executor == null)
			{
				return (EventExecutor)channel().eventLoop();
			}
			else
			{
				return _executor;
			}
		}

		public ChannelInboundInvoker fireChannelRegistered()
		{
			invokeChannelRegistered(findContextInbound());
			return this;
		}

		public static void invokeChannelActive(AbstractChannelHandlerContext next)
		{
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeChannelActive();
			}
			else
				executor.execute(() => next.invokeChannelActive());
		}

		public ChannelInboundInvoker fireChannelActive()
		{
			invokeChannelActive(findContextInbound());
			return this;
		}

		public static void invokeChannelRead(AbstractChannelHandlerContext next, Object msg)
		{
			Debug.Assert(msg != null);
			Object m = next._pipeline.touch(msg, next);
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeChannelRead(m);
			}
			else
			{
				executor.execute(() => next.invokeChannelRead(m));
			}
		}

		public ChannelInboundInvoker fireChannelRead(Object msg)
		{
			invokeChannelRead(findContextInbound(), msg);
			return this;
		}

		public static void invokeChannelReadComplete(AbstractChannelHandlerContext next)
		{
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeChannelReadComplete();
			}
			else
				executor.execute(() => next.invokeChannelReadComplete());
		}

		public ChannelInboundInvoker fireChannelReadComplete()
		{
			invokeChannelReadComplete(findContextInbound());
			return this;
		}

		public static void invokeChannelInactive(AbstractChannelHandlerContext next)
		{
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeChannelInactive();
			}
			else
				executor.execute(() => next.invokeChannelInactive());
		}

		public ChannelInboundInvoker fireChannelInactive()
		{
			invokeChannelInactive(findContextInbound());
			return this;
		}

		public static void invokeChannelUnregistered(AbstractChannelHandlerContext next)
		{
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeChannelUnregistered();
			}
			else
				executor.execute(() => next.invokeChannelUnregistered());
		}

		public ChannelInboundInvoker fireChannelUnregistered()
		{
			invokeChannelUnregistered(findContextInbound());
			return this;
		}

		public static void invokeUserEventTriggered(AbstractChannelHandlerContext next, Object _event)
		{
			Debug.Assert(_event != null);
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeUserEventTriggered(_event);
			}
			else
				executor.execute(() => next.invokeUserEventTriggered(_event));
		}

		public ChannelInboundInvoker fireUserEventTriggered(Object _event)
		{
			invokeUserEventTriggered(findContextInbound(), _event);
			return this;
		}

		public static void invokeExceptionCaught(AbstractChannelHandlerContext next, Exception cause)
		{
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeExceptionCaught(cause);
			}
			else
			{
				try
				{
					executor.execute(() => next.invokeExceptionCaught(cause));
				}
				catch (Exception t)
				{
					if (logger.IsWarnEnabled)
					{
						logger.Warn(t, "Failed to submit an exceptionCaught() event.");
						logger.Warn(cause, "The exceptionCaught() event that was failed to submit was:");
					}
				}
			}
		}

		public ChannelInboundInvoker fireExceptionCaught(Exception cause)
		{
			invokeExceptionCaught(next, cause);
			return this;
		}

		private void invokeChannelRegistered()
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelInboundHandler)handler()).channelRegistered(this);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireChannelRegistered();
			}
		}

		private void invokeChannelActive()
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelInboundHandler)handler()).channelActive(this);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireChannelActive();
			}
		}

		private void invokeChannelRead(Object msg)
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelInboundHandler)handler()).channelRead(this, msg);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireChannelRead(msg);
			}
		}

		private void invokeChannelReadComplete()
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelInboundHandler)handler()).channelReadComplete(this);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireChannelReadComplete();
			}
		}

		public ChannelInboundInvoker fireChannelWritabilityChanged()
		{
			invokeChannelWritabilityChanged(findContextInbound());
			return this;
		}

		public static void invokeChannelWritabilityChanged(AbstractChannelHandlerContext next)
		{
			EventExecutor executor = next.executor();
			if (executor.inEventLoop())
			{
				next.invokeChannelWritabilityChanged();
			}
			else
			{
				executor.execute(() => next.invokeChannelWritabilityChanged());
			}
		}

		private void invokeChannelWritabilityChanged()
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelInboundHandler)handler()).channelWritabilityChanged(this);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireChannelWritabilityChanged();
			}
		}

		private void invokeChannelInactive()
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelInboundHandler)handler()).channelInactive(this);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireChannelInactive();
			}
		}

		private void invokeChannelUnregistered()
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelInboundHandler)handler()).channelUnregistered(this);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireChannelUnregistered();
			}
		}

		private void invokeUserEventTriggered(Object _event)
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelInboundHandler)handler()).userEventTriggered(this, _event);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireUserEventTriggered(_event);
			}
		}

		private void invokeChannelWrite(Object msg)
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelOutboundHandler)handler()).write(this, msg);
				}
				catch (Exception t)
				{
					notifyHandlerException(t);
				}
			}
			else
			{
				fireChannelActive();
			}
		}

		public static void invokeChannelWrite(AbstractChannelHandlerContext next, Object msg)
		{
			next.invokeChannelWrite(msg);
		}

		public void setRemoved()
		{
			handlerState = REMOVE_COMPLETE;
		}

		public void setAddComplete()
		{
			for (; ; )
			{
				int oldState = handlerState;

				if (oldState == REMOVE_COMPLETE)
					return;

				Interlocked.CompareExchange(ref handlerState, ADD_COMPLETE, oldState);
				return;
			}
		}

		public void setAddPending()
		{
			Interlocked.CompareExchange(ref handlerState, ADD_PENDING, INIT);
		}

		private bool invokeHandler()
		{
			int handlerState = this.handlerState;
			return handlerState == ADD_COMPLETE || (!ordered && handlerState == ADD_PENDING);
		}

		public bool isRemoved()
		{
			return handlerState == REMOVE_COMPLETE;
		}

		public abstract ChannelHandler handler();

		public ChannelPipeline pipeline()
		{
			return _pipeline;
		}

		private void invokeWrite0(Object msg)
		{
			try
			{
				((ChannelOutboundHandler)handler()).write(this, msg);
			}
			catch (Exception t)
			{
				notifyOutboundHandlerException(t);
			}
		}

		private void invokeWrite(Object msg)
		{
			if (invokeHandler())
			{
				invokeWrite0(msg);
			}
		}

		public void write(Object msg)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}

			AbstractChannelHandlerContext next = findContextOutbound();
			Object m = _pipeline.touch(msg, next);
			EventExecutor executor = next.executor();
			if (executor.inEventLoop())
			{
				next.invokeWrite(m);
			}
		}

		private void invokeFlush0()
		{
			try
			{
//				((ChannelOutboundHandler)handler()).flush(this);
			}
			catch (Exception t)
			{
				notifyHandlerException(t);
			}
		}

		public void bind(SocketAddress localAddress)
		{
			AbstractChannelHandlerContext next = findContextOutbound();
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeBind(localAddress);
			}
			else
			{
				safeExecute(executor, () => next.invokeBind(localAddress), null);
			}
		}

		private void invokeBind(SocketAddress localAddress)
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelOutboundHandler)handler()).bind(this, localAddress);
				}
				catch (Exception t)
				{
					notifyOutboundHandlerException(t);
				}
			}
			else
			{
				bind(localAddress);
			}
		}

		public void bind(String deviceName)
		{
			AbstractChannelHandlerContext next = findContextOutbound();
			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				next.invokeBind(deviceName);
			}
			else
			{
				safeExecute(executor, () => next.invokeBind(deviceName), null);
			}
		}

		private void invokeBind(String deviceName)
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelOutboundHandler)handler()).bind(this, deviceName);
				}
				catch (Exception t)
				{
					notifyOutboundHandlerException(t);
				}
			}
			else
			{
				bind(deviceName);
			}
		}

		private void invokeConnect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelOutboundHandler)handler()).connect(this, remoteAddress, localAddress);
				}
				catch (Exception t)
				{
					notifyOutboundHandlerException(t);
				}
			}
		}

		public void connect(SocketAddress remoteAddress)
		{
			connect(remoteAddress, null);
		}

		public void connect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			if (remoteAddress == null)
			{
				throw new NullReferenceException("remoteAddress");
			}

			AbstractChannelHandlerContext next = findContextOutbound();
			EventExecutor executor = next.executor();
			if (executor.inEventLoop())
			{
				next.invokeConnect(remoteAddress, localAddress);
			}
			else
			{
				safeExecute(executor, () => next.invokeConnect(remoteAddress, localAddress), null);
			}
		}

		private void invokeDisconnect()
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelOutboundHandler)handler()).disconnect(this);
				}
				catch (Exception t)
				{
					notifyOutboundHandlerException(t);
				}
			}
		}

		private void invokeClose()
		{
			if (invokeHandler())
			{
				try
				{
					((ChannelOutboundHandler)handler()).close(this);
				}
				catch (Exception t)
				{
					notifyOutboundHandlerException(t);
				}
			}
			else
			{
				close();
			}
		}

		private static void safeExecute(EventExecutor executor, Action runnable, Object msg)
		{
			try
			{
				executor.execute(runnable);
			}
			catch (Exception/* cause*/)
			{
// 				try
// 				{
// 					promise.setFailure(cause);
// 				}
// 				finally
// 				{
// 					if (msg != null)
// 					{
// 						ReferenceCountUtil.release(msg);
// 					}
// 				}
			}
		}

		public void close()
		{
			AbstractChannelHandlerContext next = findContextOutbound();
			EventExecutor executor = next.executor();
			if (executor.inEventLoop())
			{
				next.invokeClose();
			}
			else
			{
				safeExecute(executor, next.invokeClose, null);
			}
		}

		public void disconnect()
		{
			AbstractChannelHandlerContext next = findContextOutbound();

			EventExecutor executor = next.executor();

			if (executor.inEventLoop())
			{
				if (!channel().metadata().hasDisconnect())
				{
					next.invokeClose();
				}
				else
					next.invokeDisconnect();
			}
		}
	}
}
