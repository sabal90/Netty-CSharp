using io.netty.channel;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;

namespace io.netty.bootstrap
{
	public abstract class AbstractBootstrap<B, C>
		where B : AbstractBootstrap<B, C>
		where C : Channel
	{
		private volatile ChannelFactory<C> _channelFactory;
		private volatile ChannelHandler _handler;
		volatile EventLoopGroup _group;
		private volatile SocketAddress _localAddress;
		private static Logger logger = LogManager.GetCurrentClassLogger();

		protected AbstractBootstrap()
		{
			// Disallow extending from a different package.
		}

		protected AbstractBootstrap(AbstractBootstrap<B, C> bootstrap)
		{
			_group = bootstrap.group();
			_channelFactory = bootstrap.channelFactory();
			_handler = bootstrap.handler();
			_localAddress = bootstrap.localAddress();
		}

		public virtual B group(EventLoopGroup _group)
		{
			if (_group == null)
			{
				throw new NullReferenceException("group");
			}

			if (this._group != null)
			{
				throw new ArgumentException("group set already");
			}

			this._group = _group;
			return (B)this;
		}

		public B channel(Type channelClass)
		{
			if (channelClass == null)
			{
				throw new ArgumentNullException("channelClass");
			}

			_channelFactory = new ReflectiveChannelFactory<C>(channelClass);
			return (B)this;
		}

		public B localAddress(SocketAddress _localAddress)
		{
			this._localAddress = _localAddress;
			return (B)this;
		}

		public B localAddress(int inetPort)
		{
			this._localAddress = new IPEndPoint(IPAddress.Any, inetPort).Serialize();
			return (B)this;
		}

		public B localAddress(String inetHost, int inetPort)
		{
			IPAddress[] ipAddressList = Dns.GetHostAddresses(inetHost);

			foreach (IPAddress ipAddress in ipAddressList)
			{
				if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					this._localAddress = new IPEndPoint(ipAddress, inetPort).Serialize();
					break;
				}
			}

			return (B)this;
		}

		public B localAddress(IPAddress inetHost, int inetPort)
		{
			this._localAddress = new IPEndPoint(inetHost, inetPort).Serialize();
			return (B)this;
		}

		public B validate()
		{
			if (_group == null)
			{
				throw new ArgumentException("group not set");
			}

			if (_channelFactory == null)
			{
				throw new ArgumentException("channel or channelFactory not set");
			}

			return (B)this;
		}

		public Channel register()
		{
			validate();
			return initAndRegister();
		}

		protected Channel initAndRegister()
		{
			Channel _channel = null;

			try
			{
				_channel = _channelFactory.newChannel();
				init(_channel);
			}
			catch (Exception e)
			{
				if (_channel != null)
				{
					// channel can be null if newChannel crashed (eg SocketException("too many open files"))
//					_channel.unsafe().closeForcibly();
				}

				logger.Warn(e);
			}

			group().register(_channel);

			return _channel;
		}

		public B handler(ChannelHandler _handler)
		{
			if (_handler == null)
			{
				throw new ArgumentNullException("handler");
			}

			this._handler = _handler;

			return (B)this;
		}

		public EventLoopGroup group()
		{
			return _group;
		}

		public ChannelFactory<C> channelFactory()
		{
			return _channelFactory;
		}

		public ChannelHandler handler()
		{
			return _handler;
		}

		public SocketAddress localAddress()
		{
			return _localAddress;
		}

		public Channel bind(String deviceName)
		{
			return doBind(deviceName);
		}

		public Channel bind(int inetPort)
		{
			return bind(new IPEndPoint(IPAddress.Any, inetPort).Serialize());
		}

		public Channel bind(SocketAddress localAddress)
		{
			validate();
			if (localAddress == null)
			{
				throw new NullReferenceException("localAddress");
			}

			return doBind(localAddress);
		}

		private Channel doBind(SocketAddress localAddress)
		{
			Channel _channel = initAndRegister();
			doBind0(_channel, localAddress);
			return _channel;
		}

		private Channel doBind(String deviceName)
		{
			Channel _channel = initAndRegister();
			doBind0(_channel, deviceName);
			return _channel;
		}

		private void doBind0(Channel _channel, SocketAddress _localAddress)
		{
			_channel.bind(_localAddress);
		}

		private void doBind0(Channel _channel, String deviceName)
		{
			_channel.bind(deviceName);
		}

		protected abstract void init(Channel _channel);
	}
}
