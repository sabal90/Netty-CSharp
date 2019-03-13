using io.netty.channel;
using io.netty.util;
using io.netty.util.inner;
using System;
using System.Collections.Generic;

namespace io.netty.handler.codec
{
	public abstract class MessageToMessageDecoder<I> : ChannelInboundHandlerAdapter
	{
		private AbstractTypeParameterMatcher matcher;

		/**
		 * Create a new instance which will try to detect the types to match out of the type parameter of the class.
		 */
		protected MessageToMessageDecoder()
		{
			matcher = AbstractTypeParameterMatcher.find(this, typeof(MessageToMessageDecoder<>), "I");
		}

		/**
		 * Create a new instance
		 *
		 * @param inboundMessageType    The type of messages to match and so decode
		 */
		protected MessageToMessageDecoder(Type inboundMessageType)
		{
			matcher = AbstractTypeParameterMatcher.get(inboundMessageType);
		}

		/**
		 * Returns {@code true} if the given message should be handled. If {@code false} it will be passed to the next
		 * {@link ChannelInboundHandler} in the {@link ChannelPipeline}.
		 */
		public virtual bool acceptInboundMessage(Object msg)
		{
			return matcher.match(msg);
		}

		public override void channelRead(ChannelHandlerContext ctx, Object msg)
		{
//			CodecOutputList outBuffers = CodecOutputList.newInstance();

			try
			{
				if (acceptInboundMessage(msg))
				{
					I cast = (I)msg;

					try
					{
//						decode(ctx, cast, outBuffers);
					}
					finally
					{
						ReferenceCountUtil.release(cast);
					}
				}
				else
				{
//					outBuffers.add(msg);
				}
			}
			catch (DecoderException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new DecoderException("channelRead", e);
			}
			finally
			{
// 				int size = outBuffers.size();
// 				for (int i = 0; i < size; i++)
// 				{
// 					ctx.fireChannelRead(outBuffers.getUnsafe(i));
// 				}
// 				outBuffers.recycle();
			}
		}

		/**
		 * Decode from one message to an other. This method will be called for each written message that can be handled
		 * by this encoder.
		 *
		 * @param ctx           the {@link ChannelHandlerContext} which this {@link MessageToMessageDecoder} belongs to
		 * @param msg           the message to decode to an other one
		 * @param out           the {@link List} to which decoded messages should be added
		 * @throws Exception    is thrown if an error occurs
		 */
		public abstract void decode(ChannelHandlerContext ctx, I msg, List<Object> outBuffers);
	}
}
