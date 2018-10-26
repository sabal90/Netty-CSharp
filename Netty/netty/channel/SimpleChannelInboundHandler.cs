using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace io.netty.channel
{
	public abstract class SimpleChannelInboundHandler<I> : ChannelInboundHandlerAdapter
	{
//		private readonly TypeParameterMatcher matcher;
		private bool autoRelease;

		/**
		 * see {@link #SimpleChannelInboundHandler(boolean)} with {@code true} as boolean parameter.
		 */
		protected SimpleChannelInboundHandler()
			: this(true)
		{
		}

		/**
		 * Create a new instance which will try to detect the types to match out of the type parameter of the class.
		 *
		 * @param autoRelease   {@code true} if handled messages should be released automatically by passing them to
		 *                      {@link ReferenceCountUtil#release(Object)}.
		 */
		protected SimpleChannelInboundHandler(bool autoRelease)
		{
//			matcher = TypeParameterMatcher.find(this, typeof(SimpleChannelInboundHandler), "I");
			this.autoRelease = autoRelease;
		}

		/**
		 * see {@link #SimpleChannelInboundHandler(Class, boolean)} with {@code true} as boolean value.
		 */
		protected SimpleChannelInboundHandler(Type inboundMessageType)
			: this(inboundMessageType, true)
		{
		}

		/**
		 * Create a new instance
		 *
		 * @param inboundMessageType    The type of messages to match
		 * @param autoRelease           {@code true} if handled messages should be released automatically by passing them to
		 *                              {@link ReferenceCountUtil#release(Object)}.
		 */
		protected SimpleChannelInboundHandler(Type inboundMessageType, bool autoRelease)
		{
//			matcher = TypeParameterMatcher.get(inboundMessageType);
			this.autoRelease = autoRelease;
		}

		/**
		 * Returns {@code true} if the given message should be handled. If {@code false} it will be passed to the next
		 * {@link ChannelInboundHandler} in the {@link ChannelPipeline}.
		 */
		public bool acceptInboundMessage(Object msg)
		{
			return true;
//			return matcher.match(msg);
		}

		public override void channelRead(ChannelHandlerContext ctx, Object msg)
		{
			bool release = true;

			try
			{
				if (acceptInboundMessage(msg))
				{
					I imsg = (I)msg;
					channelRead0(ctx, imsg);
				}
				else
				{
					release = false;
					ctx.fireChannelRead(msg);
				}
			}
			finally
			{
				if (autoRelease && release)
				{
//					ReferenceCountUtil.release(msg);
				}
			}
		}

		/**
		 * <strong>Please keep in mind that this method will be renamed to
		 * {@code messageReceived(ChannelHandlerContext, I)} in 5.0.</strong>
		 *
		 * Is called for each message of type {@link I}.
		 *
		 * @param ctx           the {@link ChannelHandlerContext} which this {@link SimpleChannelInboundHandler}
		 *                      belongs to
		 * @param msg           the message to handle
		 * @throws Exception    is thrown if an error occurred
		 */
		protected abstract void channelRead0(ChannelHandlerContext ctx, I msg);
	}
}
