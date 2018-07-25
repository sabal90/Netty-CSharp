using io.netty.buffer;
using io.netty.channel.nio;
using NLog;
using System;
using System.IO.Ports;
using System.Net;
using System.Threading;

namespace io.netty.channel.rxtx
{
	public class RxtxChannel : AbstractChannel
	{
		private SerialPort commPort;
		protected String portName;
		protected bool isTryReconnect;
		protected int tryReconnectTime;
		private static ChannelMetadata METADATA = new ChannelMetadata(true, 16);
		protected bool _isActive;
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public RxtxChannel() { }
		public RxtxChannel(String portName, bool isTryReconnect = true, int tryReconnectTime = 1000) : base(null)
		{
			_isActive = false;
			this.isTryReconnect = isTryReconnect;
			this.tryReconnectTime = tryReconnectTime;
			this.portName = portName;
			commPort = new SerialPort();
		}

		protected override SocketAddress localAddress0()
		{
			return null;
		}

		protected override SocketAddress remoteAddress0()
		{
			return null;
		}

		public override ChannelMetadata metadata()
		{
			return METADATA;
		}

		protected override AbstractUnsafe newUnsafe()
		{
			return new SerialCommChannelUnsafe(this);
		}

		protected override bool isCompatible(EventLoop loop)
		{
			return loop is NioEventLoop;
		}

		public override EventLoop eventLoop()
		{
			return base.eventLoop();
		}

		private class SerialCommChannelUnsafe : AbstractUnsafe
		{
			public SerialCommChannelUnsafe(AbstractChannel channel) : base(channel) { }
		}

		protected void Init()
		{
			SerialPortFixer.Execute(portName);
			commPort = new SerialPort();
			connect();
		}

		private void TryReconnect()
		{
			if (isTryReconnect)
			{
				Thread.Sleep(tryReconnectTime);
				Init();
			}
		}

		public void connect()
		{
			try
			{
				if (commPort == null)
					return;

				commPort.PortName = portName;
				commPort.BaudRate = 9600;
				commPort.Parity = Parity.None;
				commPort.DataBits = 8;
				commPort.StopBits = StopBits.One;
				commPort.ReadTimeout = 500;
				commPort.WriteTimeout = 500;

				commPort.DataReceived += new SerialDataReceivedEventHandler(Read);
//				commPort.ErrorReceived += new SerialErrorReceivedEventHandler(Error);

				commPort.Open();
				eventLoop().execute(() =>
				{
					fireChannelActive();
					_isActive = true;
				});
			}
			catch (Exception e)
			{
				logger.Warn(e, portName);
				commPort.Dispose();
				eventLoop().execute(() => TryReconnect());
			}
		}

		protected override void doClose()
		{
			isTryReconnect = false;
			doDisconnect();
			commPort = null;
		}

		private void Error(object sender, SerialErrorReceivedEventArgs e)
		{
			doDisconnect();
		}

		private void Read(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				SerialPort sp = (SerialPort)sender;
				int bufferSize = sp.BytesToRead;

				if (bufferSize <= 0)
					return;

				byte[] buffer = new byte[bufferSize];
				sp.Read(buffer, 0, bufferSize);
				eventLoop().execute(() => fireChannelRead(buffer));
			}
			catch (Exception cause)
			{
				logger.Warn(cause.Message);
				eventLoop().execute(() =>
				{
					pipeline().fireChannelReadComplete();
					pipeline().fireExceptionCaught(cause);
					getUnsafe().close();
					_isActive = false;
					TryReconnect();
				});
			}
		}

		public override void doConnect(SocketAddress remoteAddress, SocketAddress localAddress) { }

		public override void doWrite(ByteBuf msg)
		{
			Thread.Sleep(1);
			commPort.Write(msg.GetBytes(), 0, msg.Size());
		}

		public override void doDisconnect()
		{
			_isActive = false;

			try
			{
				if(commPort.IsOpen)
					commPort.Close();

				if(commPort != null)
					commPort.Dispose();

				eventLoop().execute(() =>
				{
					fireChannelInactive();
					TryReconnect();
				});
			}
			catch (Exception e)
			{
				logger.Warn(e);
			}
		}

		public override bool isOpen()
		{
			return true;
		}

		public override bool isActive()
		{
			if (commPort == null)
				return false;

			return _isActive;
		}

		private void fireChannelActive()
		{
			pipeline().fireChannelActive();
		}

		private void fireChannelRead(Object msg)
		{
			read(msg);
		}

		private void fireChannelInactive()
		{
			pipeline().fireChannelInactive();
		}

		protected override void doBind(SocketAddress localAddress)
		{
			_isActive = false;
			portName = "COM" + ((IPEndPoint)new IPEndPoint(0, 0).Create(localAddress)).Port.ToString();
			isTryReconnect = true;
			tryReconnectTime = 1000;
			Init();
		}

		protected override void doBind(String deviceName) { }
	}
}
