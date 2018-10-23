using System;
using System.Collections.Concurrent;

namespace io.netty.channel
{
	public abstract class AbstractChannelInitializer<C> : ChannelInboundHandlerAdapter where C : Channel
	{
		private ConcurrentDictionary<ChannelHandlerContext, bool> initMap;

		public AbstractChannelInitializer()
		{
			initMap = new ConcurrentDictionary<ChannelHandlerContext, bool>();
		}

		protected abstract void initChannel(C ch);

		public override void channelRegistered(ChannelHandlerContext ctx)
		{
			if (initChannel(ctx))
			{
				// we called initChannel(...) so we need to call now pipeline.fireChannelRegistered() to ensure we not
				// miss an event.
				ctx.pipeline().fireChannelRegistered();
			}
			else
			{
				// Called initChannel(...) before which is the expected behavior, so just forward the event.
				ctx.fireChannelRegistered();
			}
		}

		public override void handlerAdded(ChannelHandlerContext ctx)
		{
			if (ctx.channel().isRegistered())
			{
				// This should always be true with our current DefaultChannelPipeline implementation.
				// The good thing about calling initChannel(...) in handlerAdded(...) is that there will be no ordering
				// surprises if a ChannelInitializer will add another ChannelInitializer. This is as all handlers
				// will be added in the expected order.
				initChannel(ctx);
			}
		}

// 		public override void exceptionCaught(ChannelHandlerContext ctx, Exception cause)
// 		{
// 			logger.warn("Failed to initialize a channel. Closing: " + ctx.channel(), cause);
// 			ctx.close();
// 		}

		private bool initChannel(ChannelHandlerContext ctx)
		{
			if (initMap.TryAdd(ctx, true) == true) // Guard against re-entrance.
			{
				try
				{
					initChannel((C)ctx.channel());
				}
				catch (Exception cause)
				{
					// Explicitly call exceptionCaught(...) as we removed the handler before calling initChannel(...).
					// We do so to prevent multiple calls to initChannel(...).
					exceptionCaught(ctx, cause);
				}
				finally
				{
					remove(ctx);
				}

				return true;
			}

			return false;
		}

		private void remove(ChannelHandlerContext ctx)
		{
			try
			{
				ChannelPipeline pipeline = ctx.pipeline();

				if (pipeline.context(this) != null)
				{
					pipeline.remove(this);
				}
			}
			finally
			{
				bool ret;
				initMap.TryRemove(ctx, out ret);
			}
		}
	}
}
