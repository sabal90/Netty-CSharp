using io.netty.util.concurrent;
using System.Net;

namespace io.netty.resolver
{
	public class DefaultAddressResolverGroup : AddressResolverGroup<SocketAddress>
	{
		public static DefaultAddressResolverGroup INSTANCE = new DefaultAddressResolverGroup();

		private DefaultAddressResolverGroup() { }

		protected override AddressResolver<SocketAddress> newResolver(EventExecutor executor)
		{
//			return new DefaultNameResolver(executor).asAddressResolver();
			return null;
		}
	}
}
