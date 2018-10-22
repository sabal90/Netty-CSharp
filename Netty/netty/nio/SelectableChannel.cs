using System;
using System.Net.Sockets;

namespace io.netty.nio
{
	public abstract class SelectableChannel : TcpClient
	{
		protected SelectableChannel() { }
		protected SelectableChannel(TcpClient socket) { Client = socket.Client; }

		public abstract bool isRegistered();
		public abstract SelectionKey keyFor(Selector sel);
		public abstract SelectionKey register(Selector sel, Object att);
	}
}
