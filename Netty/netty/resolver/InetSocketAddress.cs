using System;
using System.Net;
using System.Net.Sockets;

namespace io.netty.resolver
{
	public class InetSocketAddress : SocketAddress
	{
		// Private implementation class pointed to by all public methods.
		private class InetSocketAddressHolder
		{
			// The hostname of the Socket Address
			private String hostname;
			// The IP address of the Socket Address
			private InetAddress addr;
			// The port number of the Socket Address
			private int port;

			public InetSocketAddressHolder(String hostname, InetAddress addr, int port)
			{
				this.hostname = hostname;
				this.addr = addr;
				this.port = port;
			}

			private int getPort()
			{
				return port;
			}

			private InetAddress getAddress()
			{
				return addr;
			}

			public String getHostName()
			{
				if (hostname != null)
					return hostname;

				if (addr != null)
					return addr.getHostName();

				return null;
			}

			private String getHostString()
			{
				if (hostname != null)
					return hostname;

				if (addr != null)
				{
					if (addr.holder().getHostName() != null)
						return addr.holder().getHostName();
					else
						return addr.getHostAddress();
				}

				return null;
			}

			private bool isUnresolved()
			{
				return addr == null;
			}

			public String toString()
			{
				if (isUnresolved())
				{
					return hostname + ":" + port;
				}
				else
				{
					return addr.toString() + ":" + port;
				}
			}

// 			public bool equals(Object obj)
// 			{
// 				if (obj == null || !(obj is InetSocketAddressHolder))
// 					return false;
// 				InetSocketAddressHolder that = (InetSocketAddressHolder)obj;
// 				bool sameIP;
// 				if (addr != null)
// 					sameIP = addr.Equals(that.addr);
// 				else if (hostname != null)
// 					sameIP = (that.addr == null) &&
// 						hostname.equalsIgnoreCase(that.hostname);
// 				else
// 					sameIP = (that.addr == null) && (that.hostname == null);
// 				return sameIP && (port == that.port);
// 			}

// 			public int hashCode()
// 			{
// 				if (addr != null)
// 					return addr.hashCode() + port;
// 				if (hostname != null)
// 					return hostname.toLowerCase().hashCode() + port;
// 				return port;
// 			}
		}

		private InetSocketAddressHolder holder = null;

		private static int checkPort(int port)
		{
			if (port < 0 || port > 0xFFFF)
				throw new ArgumentException("port out of range:" + port);

			return port;
		}

		private static String checkHost(String hostname)
		{
			if (hostname == null)
				throw new ArgumentException("hostname can't be null");

			return hostname;
		}

		private int port;
		private String hostName;

		private InetSocketAddress(AddressFamily family = AddressFamily.InterNetwork) : base(family) { }
		public static InetSocketAddress Create(String host, int port)
		{
			return new InetSocketAddress(checkPort(port), checkHost(host));//new IPEndPoint(IPAddress.Parse(inetHost), inetPort).Serialize();
		}

		private InetSocketAddress(int port, String hostName) : this()
		{
			this.hostName = hostName;
			this.port = port;
		}

// 		public InetSocketAddress(int port) : this(InetAddress.anyLocalAddress(), port) { }
// 		public InetSocketAddress(InetAddress addr, int port) : this()
// 		{
// 			holder = new InetSocketAddressHolder(
// 							null,
// 							addr == null ? InetAddress.anyLocalAddress() : addr,
// 							checkPort(port));
// 		}

		public String getHostName()
		{
			return holder.getHostName();
		}
	}
}
