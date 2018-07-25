using io.netty.util.concurrent;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace io.netty.channel
{
	public class DefaultChannelPipeline : ChannelPipeline
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private AbstractChannelHandlerContext head;
		private AbstractChannelHandlerContext tail;
		private static String HEAD_NAME = generateName0(typeof(HeadContext));
		private static String TAIL_NAME = generateName0(typeof(TailContext));
		private bool registered;
		private PendingHandlerCallback pendingHandlerCallbackHead;
		private bool firstRegistration = true;
		private Channel _channel;
		private static Dictionary<Type, String> nameCaches = new Dictionary<Type, String>();
		Dictionary<EventExecutorGroup, EventExecutor> childExecutors;

		private abstract class PendingHandlerCallback
		{
			protected DefaultChannelPipeline pipeline;
			protected AbstractChannelHandlerContext ctx;
			public PendingHandlerCallback next;

			public PendingHandlerCallback(DefaultChannelPipeline pipeline, AbstractChannelHandlerContext ctx)
			{
				this.pipeline = pipeline;
				this.ctx = ctx;
			}

			public abstract void run();
		}

		private class PendingHandlerAddedTask : PendingHandlerCallback
		{
			public PendingHandlerAddedTask(DefaultChannelPipeline pipeline, AbstractChannelHandlerContext ctx)
				: base(pipeline, ctx)
			{
			}

			public override void run()
			{
				pipeline.callHandlerAdded0(ctx);
			}
		}

		private class PendingHandlerRemovedTask : PendingHandlerCallback
		{

			public PendingHandlerRemovedTask(DefaultChannelPipeline pipeline, AbstractChannelHandlerContext ctx)
				: base(pipeline, ctx)
			{
			}

			public override void run()
			{
				pipeline.callHandlerRemoved0(ctx);
			}
		}

		private class HeadContext : AbstractChannelHandlerContext, ChannelInboundHandler, ChannelOutboundHandler
		{
			private Unsafe _unsafe;
			private DefaultChannelPipeline _pipeline;

			public HeadContext(DefaultChannelPipeline pipeline)
				: base(pipeline, null, HEAD_NAME, false, true)
			{
				this._pipeline = pipeline;
				_unsafe = pipeline.channel().getUnsafe();
				setAddComplete();
			}

			public override ChannelHandler handler()
			{
				return this;
			}

			public void channelRegistered(ChannelHandlerContext ctx)
			{
				_pipeline.invokeHandlerAddedIfNeeded();
				ctx.fireChannelRegistered();
			}

			public void channelActive(ChannelHandlerContext ctx)
			{
				ctx.fireChannelActive();
			}

			public void channelRead(ChannelHandlerContext ctx, Object msg)
			{
				ctx.fireChannelRead(msg);
			}

			public void channelReadComplete(ChannelHandlerContext ctx)
			{
				ctx.fireChannelReadComplete();
			}

			public void channelInactive(ChannelHandlerContext ctx)
			{
				ctx.fireChannelInactive();
			}

			public void channelUnregistered(ChannelHandlerContext ctx)
			{
				ctx.fireChannelUnregistered();
			}

			public void userEventTriggered(ChannelHandlerContext ctx, Object _event)
			{
				ctx.fireUserEventTriggered(_event);
			}

			public void exceptionCaught(ChannelHandlerContext ctx, Exception cause)
			{
				ctx.fireExceptionCaught(cause);
			}

			public void channelWritabilityChanged(ChannelHandlerContext ctx)
			{
				ctx.fireChannelWritabilityChanged();
			}

			public void write(ChannelHandlerContext ctx, Object msg)
			{
				_unsafe.write(msg);
			}

			public void connect(ChannelHandlerContext ctx, SocketAddress remoteAddress, SocketAddress localAddress)
			{
				_unsafe.connect(remoteAddress, localAddress);
			}

			public void disconnect(ChannelHandlerContext ctx)
			{
				_unsafe.disconnect();
			}

			public void close(ChannelHandlerContext ctx)
			{
				_unsafe.close();
			}

			public void bind(ChannelHandlerContext ctx, SocketAddress localAddress)
			{
				_unsafe.bind(localAddress);
			}

			public void bind(ChannelHandlerContext ctx, String deviceName)
			{
				_unsafe.bind(deviceName);
			}

			public void handlerAdded(ChannelHandlerContext ctx) { }
			public void handlerRemoved(ChannelHandlerContext ctx) { }
		}

		private class TailContext : AbstractChannelHandlerContext, ChannelInboundHandler
		{
			private DefaultChannelPipeline _pipeline;

			public TailContext(DefaultChannelPipeline pipeline)
				: base(pipeline, null, TAIL_NAME, true, false)
			{
				_pipeline = pipeline;
				setAddComplete();
			}

			public override ChannelHandler handler()
			{
				return this;
			}

			public void channelRegistered(ChannelHandlerContext ctx) { }
			public void channelActive(ChannelHandlerContext ctx) { }
			public void channelRead(ChannelHandlerContext ctx, Object msg) { }
			public void channelReadComplete(ChannelHandlerContext ctx) { }
			public void channelInactive(ChannelHandlerContext ctx) { }
			public void channelUnregistered(ChannelHandlerContext ctx) { }
			public void channelWritabilityChanged(ChannelHandlerContext ctx) { }
			public void userEventTriggered(ChannelHandlerContext ctx, Object _event)
			{
//				ReferenceCountUtil.release(evt);
			}
			public void exceptionCaught(ChannelHandlerContext ctx, Exception cause)
			{
				_pipeline.onUnhandledInboundException(cause);
			}

			public void handlerAdded(ChannelHandlerContext ctx) { }
			public void handlerRemoved(ChannelHandlerContext ctx) { }
		}

		public DefaultChannelPipeline(Channel _channel)
		{
			this._channel = _channel;

			tail = new TailContext(this);
			head = new HeadContext(this);

			head.next = tail;
			tail.prev = head;

			registered = false;
		}

		public Channel channel()
		{
			return _channel;
		}

		public Object touch(Object msg, AbstractChannelHandlerContext next)
		{
			//			return touch ? ReferenceCountUtil.touch(msg, next) : msg;
			return msg;
		}

		private static void remove0(AbstractChannelHandlerContext ctx)
		{
			AbstractChannelHandlerContext prev = ctx.prev;
			AbstractChannelHandlerContext next = ctx.next;
			prev.next = next;
			next.prev = prev;
		}

		public void callHandlerAdded0(AbstractChannelHandlerContext ctx)
		{
			try
			{
				ctx.handler().handlerAdded(ctx);
				ctx.setAddComplete();
			}
			catch (Exception t)
			{
				bool removed = false;
				try
				{
					remove0(ctx);
					try
					{
						ctx.handler().handlerRemoved(ctx);
					}
					finally
					{
						ctx.setRemoved();
					}
					removed = true;
				}
				catch (Exception t2)
				{
					if (logger.IsWarnEnabled)
						logger.Warn(t2, "Failed to remove a handler: " + ctx.name());
				}

				if (removed)
				{
					fireExceptionCaught(new ChannelPipelineException(ctx.handler().GetType().Name + ".handlerAdded() has thrown an exception; removed.", t));
				}
				else
				{
					fireExceptionCaught(new ChannelPipelineException(ctx.handler().GetType().Name + ".handlerAdded() has thrown an exception; also failed to remove.", t));
				}
			}
		}

		void callHandlerRemoved0(AbstractChannelHandlerContext ctx)
		{
			try
			{
				try
				{
					ctx.handler().handlerRemoved(ctx);
				}
				finally
				{
					ctx.setRemoved();
				}
			}
			catch (Exception ex)
			{
				fireExceptionCaught(new ChannelPipelineException("{ctx.Handler.GetType().Name}.HandlerRemoved() has thrown an exception.", ex));
			}
		}

		private static String generateName0(Type handlerType)
		{
			return handlerType.Name + "#0";
		}

		private void checkDuplicateName(String name)
		{
			if (context0(name) != null)
			{
				throw new ArgumentException("Duplicate handler name: " + name);
			}
		}

		private AbstractChannelHandlerContext context0(String name)
		{
			AbstractChannelHandlerContext context = head.next;
			while (context != tail)
			{
				if (context.name().Equals(name))
				{
					return context;
				}

				context = context.next;
			}

			return null;
		}

		private AbstractChannelHandlerContext newContext(EventExecutorGroup group, String name, ChannelHandler handler)
		{
			return new DefaultChannelHandlerContext(this, childExecutor(group), name, handler);
		}

		private EventExecutor childExecutor(EventExecutorGroup group)
		{
			if (group == null)
			{
				return null;
			}

			Dictionary<EventExecutorGroup, EventExecutor> childExecutors = this.childExecutors;

			if (childExecutors == null)
			{
				// Use size of 4 as most people only use one extra EventExecutor.
				childExecutors = this.childExecutors = new Dictionary<EventExecutorGroup, EventExecutor>(4);
			}

			// Pin one of the child executors once and remember it so that the same child executor
			// is used to fire events for the same channel.
			EventExecutor childExecutor;
			childExecutors.TryGetValue(group, out childExecutor);

			if (childExecutor == null)
			{
				childExecutor = group.next();
				childExecutors.Add(group, childExecutor);
			}

			return childExecutor;
		}

		private void addLast0(AbstractChannelHandlerContext newCtx)
		{
			AbstractChannelHandlerContext prev = tail.prev;
			newCtx.prev = prev;
			newCtx.next = tail;
			prev.next = newCtx;
			tail.prev = newCtx;
		}

		public void invokeHandlerAddedIfNeeded()
		{
			if (firstRegistration)
			{
				firstRegistration = false;
				callHandlerAddedForAllHandlers();
			}
		}

		private void callHandlerAddedForAllHandlers()
		{
			PendingHandlerCallback pendingHandlerCallbackHead;
			lock (this)
			{
				Debug.Assert(!this.registered);
				registered = true;
				pendingHandlerCallbackHead = this.pendingHandlerCallbackHead;
				this.pendingHandlerCallbackHead = null;
			}

			PendingHandlerCallback task = pendingHandlerCallbackHead;

			while (task != null)
			{
				task.run();
				task = task.next;
			}
		}

		private void callHandlerCallbackLater(AbstractChannelHandlerContext ctx, bool added)
		{
			Debug.Assert(!registered);

			PendingHandlerCallback task = added ? (PendingHandlerCallback)new PendingHandlerAddedTask(this, ctx) : (PendingHandlerCallback)new PendingHandlerRemovedTask(this, ctx);
			PendingHandlerCallback pending = pendingHandlerCallbackHead;

			if (pending == null)
			{
				pendingHandlerCallbackHead = task;
			}
			else
			{
				while (pending.next != null)
				{
					pending = pending.next;
				}

				pending.next = task;
			}
		}

		String filterName(String name, ChannelHandler _handler)
		{
			if (name == null)
			{
				return this.generateName(_handler);
			}

			this.checkDuplicateName(name);

			return name;
		}

		string generateName(ChannelHandler _handler)
		{
			Type handlerType = _handler.GetType();
			string name;
			nameCaches.TryGetValue(handlerType, out name);

			if (name == null)
			{
				name = generateName0(handlerType);
				nameCaches.Add(handlerType, name);
			}

			if (this.context0(name) != null)
			{
				string baseName = name.Substring(0, name.Length - 1); // Strip the trailing '0'.

				for (int i = 1; ; i++)
				{
					string newName = baseName + i;

					if (this.context0(newName) == null)
					{
						name = newName;
						break;
					}
				}
			}

			return name;
		}

		public ChannelPipeline addLast(String name, ChannelHandler handler)
		{
			AbstractChannelHandlerContext newCtx;

			lock (this)
			{
				newCtx = newContext(null, filterName(name, handler), handler);
				addLast0(newCtx);

				if (!registered)
				{
					newCtx.setAddPending();
					callHandlerCallbackLater(newCtx, true);
					return this;
				}
			}

			callHandlerAdded0(newCtx);
			return this;
		}

		public ChannelPipeline addLast(ChannelHandler[] handlers)
		{
			if (handlers == null)
			{
				throw new ArgumentNullException("handlers");
			}

			foreach (ChannelHandler h in handlers)
			{
				if (h == null)
				{
					break;
				}

				addLast(null, h);
			}

			return this;
		}

		public ChannelPipeline addLast(ChannelHandler handler)
		{
			return addLast(null, handler);
		}

		public ChannelInboundInvoker fireChannelRegistered()
		{
			AbstractChannelHandlerContext.invokeChannelRegistered(head);
			return this;
		}

		public ChannelInboundInvoker fireChannelActive()
		{
			AbstractChannelHandlerContext.invokeChannelActive(head);
			return this;
		}

		public ChannelInboundInvoker fireChannelRead(Object msg)
		{
			AbstractChannelHandlerContext.invokeChannelRead(head, msg);
			return this;
		}

		public ChannelInboundInvoker fireChannelReadComplete()
		{
			AbstractChannelHandlerContext.invokeChannelReadComplete(head);
			return this;
		}

		public ChannelInboundInvoker fireChannelWritabilityChanged()
		{
			AbstractChannelHandlerContext.invokeChannelWritabilityChanged(head);
			return this;
		}

		public ChannelInboundInvoker fireChannelInactive()
		{
			AbstractChannelHandlerContext.invokeChannelInactive(head);
			return this;
		}

		public ChannelInboundInvoker fireChannelUnregistered()
		{
			AbstractChannelHandlerContext.invokeChannelUnregistered(head);
			return this;
		}

		public ChannelInboundInvoker fireUserEventTriggered(Object _event)
		{
			AbstractChannelHandlerContext.invokeUserEventTriggered(head, _event);
			return this;
		}

		public ChannelInboundInvoker fireExceptionCaught(Exception cause)
		{
			AbstractChannelHandlerContext.invokeExceptionCaught(head, cause);
			return this;
		}

		public void write(Object msg)
		{
			tail.write(msg);
		}

		public void bind(SocketAddress localAddress)
		{
			tail.bind(localAddress);
		}

		public void bind(String deviceName)
		{
			tail.bind(deviceName);
		}

		public void connect(SocketAddress remoteAddress)
		{
			tail.connect(remoteAddress);
		}

		public void connect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			tail.connect(remoteAddress, localAddress);
		}

		public void disconnect()
		{
			tail.disconnect();
		}

		public void close()
		{
			tail.close();
		}

		public ChannelHandlerContext context(ChannelHandler handlerType)
		{
			if (handlerType == null)
			{
				throw new ArgumentNullException("handlerType");
			}

			AbstractChannelHandlerContext ctx = head.next;
			for (; ; )
			{
				if (ctx == null)
				{
					return null;
				}
				if (handlerType.GetType().IsAssignableFrom(ctx.handler().GetType()))
				{
					return ctx;
				}
				ctx = ctx.next;
			}
		}

		public ChannelPipeline remove(ChannelHandler handler)
		{
			remove(getContextOrDie(handler));
			return this;
		}

		private AbstractChannelHandlerContext getContextOrDie(ChannelHandler handler)
		{
			AbstractChannelHandlerContext ctx = (AbstractChannelHandlerContext)context(handler);
			if (ctx == null)
			{
				throw new ArgumentNullException(handler.GetType().Name);
			}
			else
			{
				return ctx;
			}
		}

		private AbstractChannelHandlerContext remove(AbstractChannelHandlerContext ctx)
		{
			Debug.Assert(ctx != this.head && ctx != this.tail);

			lock (this)
			{
				remove0(ctx);

				if (!registered)
				{
					callHandlerCallbackLater(ctx, false);
					return ctx;
				}

				EventExecutor executor = ctx.executor();

				if (!executor.inEventLoop()) {
					executor.execute(() => callHandlerRemoved0(ctx));
					return ctx;
				}
			}

			callHandlerRemoved0(ctx);
			return ctx;
		}

		protected void onUnhandledInboundException(Exception cause)
		{
			logger.Warn(cause.Message);
// 			try
// 			{
// 			logger.Warn(cause, "An exceptionCaught() event was fired, and it reached at the tail of the pipeline. " +
// 							"It usually means the last handler in the pipeline did not handle the exception.");
// 			}
// 			finally
// 			{
// 				ReferenceCountUtil.release(cause);
// 			}
		}
	}
}
