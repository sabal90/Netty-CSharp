
using System.Net.Sockets;
namespace io.netty.nio
{
	public class SocketChannelImpl : SocketChannel, SelChImpl
	{
		private const int fdVal = 0;

		public SocketChannelImpl(SelectorProvider selectorProvider) : base(selectorProvider) { }
		public SocketChannelImpl(SelectorProvider selectorProvider, TcpClient socket) : base(selectorProvider, socket) { }

		public int getFDVal()
		{
			return fdVal;
		}
	}
}
