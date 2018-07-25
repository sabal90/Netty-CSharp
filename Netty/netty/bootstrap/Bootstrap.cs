using io.netty.channel;
using io.netty.resolver;
using io.netty.util.concurrent;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace io.netty.bootstrap
{
	public class Bootstrap : AbstractBootstrap<Bootstrap, Channel>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private volatile SocketAddress _remoteAddress;
		private static AddressResolverGroup<SocketAddress> DEFAULT_RESOLVER = DefaultAddressResolverGroup.INSTANCE;
		private volatile AddressResolverGroup<SocketAddress> _resolver = (AddressResolverGroup<SocketAddress>)DEFAULT_RESOLVER;
		private BootstrapConfig config;

		public Bootstrap()
		{
			config = new BootstrapConfig(this);
		}

		private Bootstrap(Bootstrap bootstrap) : base(bootstrap)
		{
			config = new BootstrapConfig(this);
			_remoteAddress = bootstrap.remoteAddress();
		}

		protected override void init(Channel _channel)
		{
			ChannelPipeline p = _channel.pipeline();
			p.addLast(handler());
		}

		public void connect()
		{
			validate();

			if (_remoteAddress == null)
			{
				throw new ArgumentException("remoteAddress not set");
			}

			doResolveAndConnect(_remoteAddress, config.localAddress());
		}

		public void connect(String ipAddress, int port)
		{
			connect(new IPEndPoint(IPAddress.Parse(ipAddress), port).Serialize());
		}

		public void connect(SocketAddress remoteAddress)
		{
			if (remoteAddress == null)
			{
				throw new NullReferenceException("remoteAddress");
			}

			validate();
			doResolveAndConnect(remoteAddress, config.localAddress());
		}

		private void doResolveAndConnect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			Channel _channel = initAndRegister();
			doResolveAndConnect0(_channel, remoteAddress, localAddress);
		}

		private void doResolveAndConnect0(Channel channel, SocketAddress remoteAddress, SocketAddress localAddress)
		{
			EventLoop eventLoop = channel.eventLoop();
			AddressResolver<SocketAddress> resolver = _resolver.getResolver((EventExecutor)eventLoop);
			doConnect(channel, remoteAddress, localAddress);
		}

		private void doConnect(Channel channel, SocketAddress remoteAddress, SocketAddress localAddress)
		{
			channel.eventLoop().execute(() => channel.connect(remoteAddress, localAddress));
		}

		public SocketAddress remoteAddress()
		{
			return _remoteAddress;
		}

		public AddressResolverGroup<SocketAddress> resolver()
		{
			return _resolver;
		}
	}
}
