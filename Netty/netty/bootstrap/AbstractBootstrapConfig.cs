using io.netty.channel;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace io.netty.bootstrap
{
	public abstract class AbstractBootstrapConfig<B, C>
		where B : AbstractBootstrap<B, C>
		where C : Channel
	{
		protected B bootstrap;

		protected AbstractBootstrapConfig(B bootstrap)
		{
			Debug.Assert(bootstrap != null);
			this.bootstrap = bootstrap;
		}

		/**
		 * Returns the configured local address or {@code null} if non is configured yet.
		 */
		public SocketAddress localAddress()
		{
			return bootstrap.localAddress();
		}

		/**
		 * Returns the configured {@link ChannelFactory} or {@code null} if non is configured yet.
		 */
		public ChannelFactory<C> channelFactory()
		{
			return bootstrap.channelFactory();
		}

		/**
		 * Returns the configured {@link ChannelHandler} or {@code null} if non is configured yet.
		 */
		public ChannelHandler handler()
		{
			return bootstrap.handler();
		}

		/**
		 * Returns the configured {@link EventLoopGroup} or {@code null} if non is configured yet.
		 */
		public EventLoopGroup group()
		{
			return bootstrap.group();
		}

		public virtual String toString()
		{
			StringBuilder buf = new StringBuilder()
					.Append(this.GetType().Name)
					.Append('(');
			EventLoopGroup _group = group();
			if (_group != null)
			{
				buf.Append("group: ")
						.Append(_group.GetType().Name)
						.Append(", ");
			}

			SocketAddress _localAddress = localAddress();
			if (_localAddress != null)
			{
				buf.Append("localAddress: ")
						.Append(_localAddress)
						.Append(", ");
			}

			ChannelHandler _handler = handler();
			if (_handler != null)
			{
				buf.Append("handler: ")
						.Append(_handler)
						.Append(", ");
			}
			if (buf[buf.Length - 1] == '(')
			{
				buf.Append(')');
			}
			else
			{
				buf[buf.Length - 2] = ')';
				buf.Length = (buf.Length - 1);
			}
			return buf.ToString();
		}
	}
}
