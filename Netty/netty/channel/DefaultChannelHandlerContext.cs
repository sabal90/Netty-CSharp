using io.netty.util.concurrent;
using System;

namespace io.netty.channel
{
	public class DefaultChannelHandlerContext : AbstractChannelHandlerContext
	{
		private ChannelHandler _handler;

		public DefaultChannelHandlerContext(DefaultChannelPipeline pipeline, EventExecutor executor, String name, ChannelHandler _handler)
			: base(pipeline, executor, name, isInbound(_handler), isOutbound(_handler))
		{
			if (_handler == null)
			{
				throw new ArgumentNullException("handler");
			}

			this._handler = _handler;
		}

		public override ChannelHandler handler()
		{
			return _handler;
		}

		private static bool isInbound(ChannelHandler _handler)
		{
			return typeof(ChannelInboundHandler).IsAssignableFrom(_handler.GetType());
		}

		private static bool isOutbound(ChannelHandler _handler)
		{
			return typeof(ChannelOutboundHandler).IsAssignableFrom(_handler.GetType());
		}
	}
}
