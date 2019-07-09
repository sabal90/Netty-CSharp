using io.netty.channel.nio;
using io.netty.nio;
using io.netty.buffer;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace io.netty.channel.socket.nio
{
	public class NioSocketChannel : AbstractChannel, io.netty.channel.socket.SocketChannel
	{
		private static SelectorProvider DEFAULT_SELECTOR_PROVIDER = SelectorProvider.provider();

		private TcpClient tcpClient;
		private int tryReconnectTime;
		private bool isTryReconnect;
		private byte[] buffer;
		private static ChannelMetadata METADATA = new ChannelMetadata(true, 16);
		private bool _isActive;
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static io.netty.nio.SocketChannel newSocket(SelectorProvider provider)
		{
			try
			{
				/**
				 *  Use the {@link SelectorProvider} to open {@link SocketChannel} and so remove condition in
				 *  {@link SelectorProvider#provider()} which is called by each SocketChannel.open() otherwise.
				 *
				 *  See <a href="https://github.com/netty/netty/issues/2308">#2308</a>.
				 */
				return provider.openSocketChannel();
			}
			catch (IOException e)
			{
				throw new ChannelException("Failed to open a socket.", e);
			}
		}

		public NioSocketChannel() : this(DEFAULT_SELECTOR_PROVIDER) { }
		public NioSocketChannel(SelectorProvider provider) : this(newSocket(provider)) { }
		public NioSocketChannel(io.netty.nio.SocketChannel socket) : this(null, socket) { }
		public NioSocketChannel(Channel parent, io.netty.nio.SocketChannel socket) : base(parent, socket)
		{
			if (parent == null)
			{
				_isActive = false;
				isTryReconnect = true;
				tryReconnectTime = 1000;
			}
			else
			{
				_isActive = true;
				_localAddress = socket.Client.LocalEndPoint.Serialize();
				_remoteAddress = socket.Client.RemoteEndPoint.Serialize();
			}

			this.tcpClient = socket;
			buffer = new byte[1024];
		}

		public NioSocketChannel(SocketAddress remoteAddress, SocketAddress localAddress, bool isTryReconnect = true, int tryReconnectTime = 1000)
			: this()
		{
			this.tryReconnectTime = tryReconnectTime;
			this.isTryReconnect = isTryReconnect;

			_remoteAddress = remoteAddress;
			_localAddress = localAddress;

			tcpClient = new TcpClient();
		}

		protected override SocketAddress localAddress0()
		{
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress myself = host.AddressList[0];

			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return new IPEndPoint(ip, 0).Serialize();
				}
			}

			return null;
		}

		protected override SocketAddress remoteAddress0()
		{
			return _remoteAddress;
		}

		public override ChannelMetadata metadata()
		{
			return METADATA;
		}

		protected override AbstractUnsafe newUnsafe()
		{
			return new AsyncTcpChannelUnsafe(this);
		}

		protected override bool isCompatible(EventLoop loop)
		{
			return loop is NioEventLoop;
		}

		public override EventLoop eventLoop()
		{
			return base.eventLoop();
		}

		private class AsyncTcpChannelUnsafe : AbstractUnsafe
		{
			public AsyncTcpChannelUnsafe(AbstractChannel channel) : base(channel) {}

			protected override void doDeregister()
			{
//				eventLoop().cancel();
			}
		}

		private void Init()
		{
			if (tcpClient != null)
			{
//				tcpClient.Client.Dispose();
				_localAddress = null;
				tcpClient.Client.Close();
				tcpClient.Close();
				tcpClient = null;
			}

			tcpClient = new TcpClient();
			connect();
		}

		private void TryReconnect()
		{
			if (isTryReconnect)
			{
				Thread.Sleep(tryReconnectTime);
				connect();
			}
		}

		public void connect()
		{
			try
			{
				IPEndPoint ipEndPoint = (IPEndPoint)new IPEndPoint(0, 0).Create(_remoteAddress);
				tcpClient.BeginConnect(ipEndPoint.Address, ipEndPoint.Port, new AsyncCallback(ConnectCallback), tcpClient);
			}
			catch (SocketException e)
			{
				logger.Warn(e.Message);
				Init();
			}
			catch (ObjectDisposedException e)
			{
				logger.Warn(e.Message);
				Init();
			}
			catch (NullReferenceException)
			{
			}
		}

		protected override void doClose()
		{
			isTryReconnect = false;
			doDisconnect();
			tcpClient = null;
		}

		public override bool isActive()
		{
			return _isActive;
		}

		public override bool isOpen()
		{
			return true;
		}

		public void Read()
		{
			tcpClient.GetStream().BeginRead(buffer, 0, buffer.Length, new AsyncCallback(ReceiveCallback), tcpClient);
		}

		public override void doConnect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			_remoteAddress = remoteAddress;
			_localAddress = localAddress;

			connect();
		}

		public override void doWrite(ByteBuf msg)
		{
			write(msg.GetBytes(), 0, msg.Size());
		}

		public override void doDisconnect()
		{
			_isActive = false;

			try
			{
				_localAddress = null;
				tcpClient.GetStream().Dispose();
//				tcpClient.Client.Dispose();
				tcpClient.Client.Close();
				tcpClient.Close();
			}
			catch (Exception e)
			{
				logger.Warn(e.Message);
			}
		}

		public void write(byte[] msg, int offset, int length)
		{
			try
			{
				if (tcpClient != null && tcpClient.Connected)
					tcpClient.GetStream().BeginWrite(msg, offset, length, new AsyncCallback(SendCallback), tcpClient);
			}
			catch (Exception e)
			{
				logger.Warn(e.Message);

				if (e is IOException)
				{
					if (isActive() && tcpClient.Connected == false)
					{
						getUnsafe().disconnect();
						TryReconnect();
					}
				}
			}
		}

		public void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				tcpClient.EndConnect(ar);
				_localAddress = tcpClient.Client.LocalEndPoint.Serialize();

				eventLoop().execute(() =>
				{
					fireChannelActive();
					_isActive = true;
					Read();
				});
			}
			catch (Exception e)
			{
				logger.Info(e.Message);
				TryReconnect();
			}
		}

		public void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				int len = tcpClient.GetStream().EndRead(ar);

				if (len > 0)
				{
					byte[] rcvBuffer = new byte[len];
					Array.Copy(buffer, rcvBuffer, len);
					eventLoop().execute(() => fireChannelRead(rcvBuffer));
				}
				else
				{
					getUnsafe().disconnect();
					TryReconnect();
				}
			}
			catch (Exception cause)
			{
				pipeline().fireChannelReadComplete();
				pipeline().fireExceptionCaught(cause);

				if (isActive() && tcpClient.Connected == false)
				{
					getUnsafe().disconnect();
					TryReconnect();
				}
			}
		}

		public void SendCallback(IAsyncResult ar)
		{
			try
			{
				tcpClient.GetStream().EndWrite(ar);
			}
			catch (Exception e)
			{
				logger.Warn(e.Message);
			}
		}

		private void fireChannelActive()
		{
			pipeline().fireChannelActive();
		}

		private void fireChannelRead(Object msg)
		{
			read(msg);
		}

		protected override void doBind(SocketAddress localAddress) { }
		protected override void doBind(String deviceName) { }
	}
}
