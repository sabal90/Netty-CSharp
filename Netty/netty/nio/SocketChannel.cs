using System.Net.Sockets;

namespace io.netty.nio
{
	public abstract class SocketChannel : AbstractSelectableChannel
	{
		protected SocketChannel(SelectorProvider provider) : base(provider) { }
		protected SocketChannel(SelectorProvider provider, TcpClient socket) : base(provider, socket) { }
	}
}
