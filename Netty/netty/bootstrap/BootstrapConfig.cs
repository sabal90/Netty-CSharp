using io.netty.channel;
using io.netty.resolver;
using System;
using System.Net;
using System.Text;

namespace io.netty.bootstrap
{
	public class BootstrapConfig : AbstractBootstrapConfig<Bootstrap, Channel>
	{
		public BootstrapConfig(Bootstrap bootstrap) : base(bootstrap) { }

		/**
		* Returns the configured remote address or {@code null} if non is configured yet.
		*/
		public SocketAddress remoteAddress()
		{
			return bootstrap.remoteAddress();
		}

		public AddressResolverGroup<SocketAddress> resolver()
		{
			return bootstrap.resolver();
		}

		public override String toString()
		{
			StringBuilder buf = new StringBuilder(base.toString());
			buf.Length = buf.Length - 1;
			buf.Append(", resolver: ").Append(resolver());
			SocketAddress _remoteAddress = remoteAddress();
			if (_remoteAddress != null)
			{
				buf.Append(", remoteAddress: ")
						.Append(_remoteAddress);
			}
			return buf.Append(')').ToString();
		}
	}
}
