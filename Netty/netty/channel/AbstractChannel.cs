using io.netty.buffer;
using io.netty.channel.nio;
using io.netty.nio;
using io.netty.util.concurrent;
using NLog;
using System;
using System.Net;

namespace io.netty.channel
{
	public abstract class AbstractChannel : Channel
	{
		protected Channel _parent;
		private ChannelId _id;
		private DefaultChannelPipeline _pipeline;
		protected volatile bool registered;
		private bool neverRegistered = true;
		private Unsafe _unsafe;
		private Object receiveMsg;
		private volatile EventLoop _eventLoop;
		protected volatile SocketAddress _localAddress;
		protected volatile SocketAddress _remoteAddress;
		private static ClosedChannelException CLOSE_CLOSED_CHANNEL_EXCEPTION = new ClosedChannelException("close(...)");
		private SelectableChannel ch;
		private volatile SelectionKey _selectionKey;
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public AbstractChannel()
		{
			_id = newId();
			_unsafe = newUnsafe();
			_pipeline = newChannelPipeline();
		}

		protected AbstractChannel(Channel _parent, SelectableChannel ch = null) : this()
		{
			this._parent = _parent;
			this.ch = ch;
		}

		public virtual Unsafe getUnsafe()
		{
			return _unsafe;
		}

		public ChannelPipeline pipeline()
		{
			return _pipeline;
		}

		protected virtual void doRegister()
		{
			if (windowsChannel() == null)
				return;

			_selectionKey = windowsChannel().register(((NioEventLoop)(eventLoop())).unwrappedSelector(), this);
		}

		public ChannelId id()
		{
			return _id;
		}

		public Channel parent()
		{
			return _parent;
		}

		protected ChannelId newId()
		{
			return DefaultChannelId.newInstance();
		}

		protected DefaultChannelPipeline newChannelPipeline()
		{
			return new DefaultChannelPipeline(this);
		}

		public SelectableChannel windowsChannel()
		{
			return ch;
		}

		public virtual EventLoop eventLoop()
		{
			EventLoop eventLoop = this._eventLoop;

			if (eventLoop == null)
			{
				throw new ArgumentException("channel not registered to an event loop");
			}
			return eventLoop;
		}

		protected SelectionKey selectionKey()
		{
			if (_selectionKey == null)
				throw new Exception("selectionKey is null");

			return _selectionKey;
		}

		public SocketAddress localAddress()
		{
			if (_localAddress == null)
			{
				try
				{
					_localAddress = getUnsafe().localAddress();
				}
				catch (Exception)
				{
					return null;
				}
			}

			return _localAddress;
		}

		public SocketAddress remoteAddress()
		{
			if (_remoteAddress == null)
			{
				try
				{
					_remoteAddress = getUnsafe().remoteAddress();
				}
				catch (Exception)
				{
					return null;
				}
			}

			return _remoteAddress;
		}

		public bool isRegistered()
		{
			return registered;
		}

		public void write(Object msg)
		{
			msg = filterOutboundMessage(msg);
			_pipeline.write(msg);
		}

		protected virtual Object filterOutboundMessage(Object msg)
		{
			return msg;
		}

		protected void read(Object msg)
		{
			eventLoop().execute(() =>
			{
				receiveMsg = msg;
				_unsafe.read();
			});
		}

		public void bind(SocketAddress localAddress)
		{
			bool wasActive = isActive();
			doBind(localAddress);
		}

		public void bind(String deviceName)
		{
			bool wasActive = isActive();
			doBind(deviceName);
		}

		public void connect(SocketAddress remoteAddress)
		{
			_pipeline.connect(remoteAddress);
		}

		public void connect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			_pipeline.connect(remoteAddress, localAddress);
		}

		public void disconnect()
		{
			_pipeline.disconnect();
		}

		public void close()
		{
			_pipeline.close();
		}

		public abstract ChannelMetadata metadata();
		protected abstract AbstractUnsafe newUnsafe();
		protected abstract bool isCompatible(EventLoop loop);
		protected abstract void doClose();
		protected abstract void doBind(SocketAddress localAddress);
		protected abstract void doBind(String deviceName);
		protected abstract SocketAddress localAddress0();
		protected abstract SocketAddress remoteAddress0();
		public abstract void doWrite(ByteBuf msg);
		public abstract void doConnect(SocketAddress remoteAddress, SocketAddress localAddress);
		public abstract void doDisconnect();
		public abstract bool isOpen();
		public abstract bool isActive();

		protected abstract class AbstractUnsafe : Unsafe
		{
			protected AbstractChannel channel;

			protected AbstractUnsafe(AbstractChannel channel)
			{
				this.channel = channel;
			}

			public SocketAddress localAddress()
			{
				return channel.localAddress0();
			}

			public SocketAddress remoteAddress()
			{
				return channel.remoteAddress0();
			}

			public void register(EventLoop _eventLoop)
			{
				if (_eventLoop == null)
				{
					throw new NullReferenceException("eventLoop");
				}

				if (channel.isRegistered())
					return;

				if (!channel.isCompatible(_eventLoop))
					return;

				channel._eventLoop = _eventLoop;

				if (((EventExecutor)_eventLoop).inEventLoop())
				{
					register0();
				}
				else
				{
					_eventLoop.execute(() => register0());
				}
			}

			private void register0()
			{
				try
				{
					bool firstRegistration = channel.neverRegistered;
					channel.doRegister();
					channel.neverRegistered = false;
					channel.registered = true;

					channel._pipeline.invokeHandlerAddedIfNeeded();
					channel.pipeline().fireChannelRegistered();

					if (channel.isActive())
					{
						if (firstRegistration)
							channel.pipeline().fireChannelActive();
					}
				}
				catch (Exception e)
				{
					// Close the channel directly to avoid FD leak.
					closeForcibly();
// 					closeFuture.setClosed();
// 					safeSetFailure(promise, t);
					logger.Info(e);
				}
			}

			public void bind(SocketAddress localAddress)
			{
				bool wasActive = channel.isActive();
				channel.doBind(localAddress);

				if (!wasActive && channel.isActive())
				{
					invokeLater(() => channel.pipeline().fireChannelActive());
				}
			}

			public void bind(String deviceName)
			{
				bool wasActive = channel.isActive();
				channel.doBind(deviceName);

				if (!wasActive && channel.isActive())
				{
					invokeLater(() => channel.pipeline().fireChannelActive());
				}
			}

			public void connect(SocketAddress remoteAddress, SocketAddress localAddress)
			{
				channel.doConnect(remoteAddress, localAddress);
			}

			public void read()
			{
				Object msg = channel.receiveMsg;
				channel.pipeline().fireChannelRead(msg);
				channel.pipeline().fireChannelReadComplete();
			}

			public void write(Object msg)
			{
				if (!typeof(ByteBuf).IsAssignableFrom(msg.GetType()))
					throw new Exception("unsupported message type:" + msg.GetType().Name);

				channel.doWrite((ByteBuf)msg);
			}

			public void disconnect()
			{
				bool wasActive = channel.isActive();
				channel.doDisconnect();

				if (wasActive && !channel.isActive())
				{
					invokeLater(() => channel.pipeline().fireChannelInactive());
				}
			}

			public void close()
			{
				close(CLOSE_CLOSED_CHANNEL_EXCEPTION, CLOSE_CLOSED_CHANNEL_EXCEPTION, false);
			}

			private void close(Exception cause, ClosedChannelException closeCause, bool notify)
			{
				bool wasActive = channel.isActive();
				doClose0();
				fireChannelInactiveAndDeregister(wasActive);
			}

			private void doClose0()
			{
				try
				{
					channel.doClose();
				}
				catch (Exception/* t*/)
				{
				}
			}

			private void fireChannelInactiveAndDeregister(bool wasActive)
			{
				deregister(wasActive && !channel.isActive());
			}

			public void closeForcibly()
			{
				try
				{
					channel.doClose();
				}
				catch (Exception e)
				{
					logger.Warn(e, "Failed to close a channel.");
				}
			}

			private void deregister(bool fireChannelInactive)
			{
				if (!channel.registered)
				{
					return;
				}

				WindowsSelectorImpl windowsSelector = ((WindowsSelectorImpl)((NioEventLoop)(channel.eventLoop())).unwrappedSelector());
				windowsSelector.deRegister((SelectionKeyImpl)channel.selectionKey());

				// As a user may call deregister() from within any method while doing processing in the ChannelPipeline,
				// we need to ensure we do the actual deregister operation later. This is needed as for example,
				// we may be in the ByteToMessageDecoder.callDecode(...) method and so still try to do processing in
				// the old EventLoop while the user already registered the Channel to a new EventLoop. Without delay,
				// the deregister operation this could lead to have a handler invoked by different EventLoop and so
				// threads.
				//
				// See:
				// https://github.com/netty/netty/issues/4435

				invokeLater(() =>
				{
					try
					{
						doDeregister();
					}
					catch (Exception t)
					{
						logger.Warn(t, "Unexpected exception occurred while deregistering a channel.");
					}
					finally
					{
						if (fireChannelInactive)
						{
							channel.pipeline().fireChannelInactive();
						}
						// Some transports like local and AIO does not allow the deregistration of
						// an open channel.  Their doDeregister() calls close(). Consequently,
						// close() calls deregister() again - no need to fire channelUnregistered, so check
						// if it was registered.
						if (channel.registered)
						{
							channel.registered = false;
							channel.pipeline().fireChannelUnregistered();
						}
					}
				});
			}

			private void invokeLater(Action task)
			{
				try
				{
					// This method is used by outbound operation implementations to trigger an inbound event later.
					// They do not trigger an inbound event immediately because an outbound operation might have been
					// triggered by another inbound event handler method.  If fired immediately, the call stack
					// will look like this for example:
					//
					//   handlerA.inboundBufferUpdated() - (1) an inbound handler method closes a connection.
					//   -> handlerA.ctx.close()
					//      -> channel.unsafe.close()
					//         -> handlerA.channelInactive() - (2) another inbound handler method called while in (1) yet
					//
					// which means the execution of two inbound handler methods of the same handler overlap undesirably.
					channel.eventLoop().execute(task);
				}
				catch (Exception e)
				{
					logger.Warn(e, "Can't invoke task later as EventLoop rejected it");
				}
			}

			protected virtual void doDeregister() { }
		}
	}
}
