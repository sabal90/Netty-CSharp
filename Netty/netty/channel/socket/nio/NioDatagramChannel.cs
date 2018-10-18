using io.netty.channel.nio;
using io.netty.buffer;
using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace io.netty.channel.socket.nio
{
	public class NioDatagramChannel : AbstractChannel, DatagramChannel
	{
		public enum NetworkType
		{
			CLIENT = 0,
			SERVER = 1
		}

		private UdpClient udpClient;
		private int tryReconnectTime;
		private bool isTryReconnect;
		private NetworkType networkType;
		private static ChannelMetadata METADATA = new ChannelMetadata(false, 16);
		private bool _isActive;
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public NioDatagramChannel() : base(null) { }
		public NioDatagramChannel(int port, bool isTryReconnect = true, int tryReconnectTime = 1000) : base(null)
		{
			_isActive = false;
			networkType = NetworkType.SERVER;
		}

		public NioDatagramChannel(SocketAddress remoteAddress, SocketAddress localAddress, bool isTryReconnect = true, int tryReconnectTime = 1000)
			: base(null)
		{
			this.tryReconnectTime = tryReconnectTime;
			this.isTryReconnect = isTryReconnect;
			this.networkType = NetworkType.CLIENT;

			_remoteAddress = remoteAddress;
			_localAddress = localAddress;
		}

		protected override SocketAddress localAddress0()
		{
			return _localAddress;
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
			return new AsyncUdpChannelUnsafe(this);
		}

		protected override bool isCompatible(EventLoop loop)
		{
			return loop is NioEventLoop;
		}

		public override EventLoop eventLoop()
		{
			return base.eventLoop();
		}

		private class AsyncUdpChannelUnsafe : AbstractUnsafe
		{
			public AsyncUdpChannelUnsafe(AbstractChannel channel) : base(channel) { }
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
				switch (networkType)
				{
					case NetworkType.CLIENT:
						udpClient = new UdpClient();
						break;
					case NetworkType.SERVER:
						udpClient = new UdpClient((IPEndPoint)new IPEndPoint(0, 0).Create(_localAddress));
						Read();
						break;
					default:
						break;
				}
			}
			catch (SocketException e)
			{
				logger.Warn(e.Message);
				return;
			}
			catch (ObjectDisposedException e)
			{
				logger.Warn(e.Message);
				return;
			}
			catch (NullReferenceException)
			{
				return;
			}

			registered = true;
			_isActive = true;
			fireChannelActive();
		}

		protected override void doClose()
		{
			isTryReconnect = false;
			doDisconnect();
			udpClient = null;
		}

		public override bool isOpen()
		{
			return true;
		}

		public override bool isActive()
		{
			return _isActive;
		}

		protected void Read()
		{
			udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpClient);
		}

		public override void doConnect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			_isActive = false;
			networkType = NetworkType.CLIENT;

			_remoteAddress = remoteAddress;
			_localAddress = localAddress;

			this.isTryReconnect = true;
			tryReconnectTime = 1000;

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
				udpClient.Close();
			}
			catch (Exception e)
			{
				logger.Warn(e.Message);
			}
		}

		protected override Object filterOutboundMessage(Object msg)
		{
			return msg;
// 			if (msg is DatagramPacket)
// 			{
// 				DatagramPacket p = (DatagramPacket)msg;
// 				ByteBuf content = p.content();
// 				if (isSingleDirectBuffer(content))
// 				{
// 					return p;
// 				}
// 
// 				return new DatagramPacket(newDirectBuffer(p, content), p.recipient());
// 			}
// 
// 			if (msg is ByteBuf)
// 			{
// 				ByteBuf buf = (ByteBuf)msg;
// 				if (isSingleDirectBuffer(buf))
// 				{
// 					return buf;
// 				}
// 				return newDirectBuffer(buf);
// 			}
// 
// 			if (msg is AddressedEnvelope)
// 			{
// 				AddressedEnvelope<Object, SocketAddress> e = (AddressedEnvelope<Object, SocketAddress>)msg;
// 				if (e.content() is ByteBuf)
// 				{
// 					ByteBuf content = (ByteBuf)e.content();
// 					if (isSingleDirectBuffer(content))
// 					{
// 						return e;
// 					}
// 					return new DefaultAddressedEnvelope<ByteBuf, SocketAddress>(newDirectBuffer(e, content), e.recipient());
// 				}
// 			}
// 
// 			throw new UnsupportedOperationException("unsupported message type: " + StringUtil.simpleClassName(msg) + EXPECTED_TYPES);
		}

		public void write(byte[] msg, int offset, int length)
		{
			try
			{
				if (offset != 0)
					Array.Copy(msg, offset, msg, 0, length);

				udpClient.BeginSend(msg, length, (IPEndPoint)new IPEndPoint(0, 0).Create(_remoteAddress), new AsyncCallback(SendCallback), udpClient);
			}
			catch (Exception e)
			{
				logger.Warn(e.Message);
				getUnsafe().disconnect();
				TryReconnect();
			}
		}

		public void ReceiveCallback(IAsyncResult ar)
		{
			try
			{
				IPEndPoint remoteEP = new IPEndPoint(0, 0);
				byte[] receiveBytes = udpClient.EndReceive(ar, ref remoteEP);

				if (receiveBytes.Length > 0)
//					fireChannelRead(new DatagramPacket(ByteBuf.Wrap(receiveBytes), _localAddress, remoteEP.Serialize()));
					fireChannelRead(receiveBytes);

				Read();
			}
			catch (Exception cause)
			{
				logger.Warn(cause.Message);
				pipeline().fireChannelReadComplete();
				pipeline().fireExceptionCaught(cause);
				getUnsafe().disconnect();
				TryReconnect();
			}
		}

		public void SendCallback(IAsyncResult ar)
		{
			try
			{
				udpClient.EndSend(ar);
			}
			catch (Exception e)
			{
				logger.Warn(e.Message);
			}
		}

		private void fireChannelActive()
		{
			eventLoop().execute(() => pipeline().fireChannelActive());
		}

		private void fireChannelRead(Object msg)
		{
			read(msg);
		}

		protected override void doBind(SocketAddress localAddress)
		{
			this._localAddress = localAddress;
			networkType = NetworkType.SERVER;
			connect();
		}

		protected override void doBind(String deviceName) { }
	}
}
