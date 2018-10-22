using io.netty.channel.nio;
using io.netty.nio;
using io.netty.buffer;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;

namespace io.netty.channel.socket.nio
{
	public class NioServerSocketChannel : AbstractChannel, ServerSocketChannel
	{
		private TcpListener tcpListener;
		private bool _isActive;
//		private SocketAddress _localAddress;
		private static ChannelMetadata METADATA = new ChannelMetadata(false, 16);
		private static SelectorProvider DEFAULT_SELECTOR_PROVIDER = SelectorProvider.provider();
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public NioServerSocketChannel() : base(null)
		{
			_isActive = true;
		}

		private void init()
		{
			try
			{
				tcpListener = new TcpListener((IPEndPoint)new IPEndPoint(0, 0).Create(_localAddress));
				tcpListener.Start();
				tcpListener.BeginAcceptTcpClient(new AsyncCallback(onAccept), tcpListener);
			}
			catch (Exception e)
			{
				logger.Warn(e.Message);
			}
		}

		public NioServerSocketChannel(SocketAddress localAddress, bool isTryReconnect = true, int tryReconnectTime = 1000)
			: this()
		{
			_localAddress = localAddress;
			init();
		}

		protected override SocketAddress localAddress0()
		{
			return _localAddress;
		}

		protected override SocketAddress remoteAddress0()
		{
			return null;
		}

		private void onAccept(IAsyncResult ar)
		{
			try
			{
// 				TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
// 				SocketChannelImpl socketChannel = new SocketChannelImpl(DEFAULT_SELECTOR_PROVIDER, tcpClient);
				NioSocketChannel remoteChannel = new NioSocketChannel(this, new SocketChannelImpl(DEFAULT_SELECTOR_PROVIDER, tcpListener.EndAcceptTcpClient(ar)));
				pipeline().fireChannelRead(remoteChannel);
				pipeline().fireChannelReadComplete();
				remoteChannel.Read();
				tcpListener.BeginAcceptTcpClient(new AsyncCallback(onAccept), tcpListener);
			}
			catch (Exception e)
			{
				logger.Warn(e.Message);
				_isActive = false;
				tcpListener.Stop();
				init();
			}
		}

		protected override AbstractUnsafe newUnsafe()
		{
			return new AsyncServerSocketChannelUnsafe(this);
		}

		private class AsyncServerSocketChannelUnsafe : AbstractUnsafe
		{
			public AsyncServerSocketChannelUnsafe(AbstractChannel channel) : base(channel) { }

			protected override void doDeregister()
			{
//				eventLoop().cancel();
			}
		}

		public override EventLoop eventLoop()
		{
			return base.eventLoop();
		}

		protected override bool isCompatible(EventLoop loop)
		{
			return loop is NioEventLoop;
		}

		protected override void doClose()
		{
			throw new NotImplementedException();
		}

		public override void doWrite(ByteBuf msg)
		{
			throw new NotImplementedException();
		}

		public override void doConnect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			throw new NotSupportedException();
		}

		public override void doDisconnect()
		{
			throw new NotSupportedException();
		}

		public override ChannelMetadata metadata()
		{
			return METADATA;
		}

		public override bool isOpen()
		{
			throw new NotImplementedException();
		}

		public override bool isActive()
		{
			return _isActive;
		}

		protected override void doBind(SocketAddress localAddress)
		{
			this._localAddress = localAddress;
			init();
		}

		protected override void doBind(String deviceName) { }
	}
}
