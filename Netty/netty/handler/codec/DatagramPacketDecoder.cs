using io.netty.buffer;
using io.netty.channel;
using io.netty.channel.socket;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace io.netty.handler.codec
{
	public class DatagramPacketDecoder : MessageToMessageDecoder<DatagramPacket>
	{
		private MessageToMessageDecoder<ByteBuf> decoder;

		/**
		 * Create a {@link DatagramPacket} decoder using the specified {@link ByteBuf} decoder.
		 *
		 * @param decoder the specified {@link ByteBuf} decoder
		 */
		public DatagramPacketDecoder(MessageToMessageDecoder<ByteBuf> decoder)
		{
			Debug.Assert(decoder != null);
			this.decoder = decoder;
		}

		public override bool acceptInboundMessage(Object msg)
		{
			if (msg is DatagramPacket)
			{
				return decoder.acceptInboundMessage(((DatagramPacket)msg).content());
			}
			return false;
		}

		public override void decode(ChannelHandlerContext ctx, DatagramPacket msg, List<Object> outBuffers)
		{
			decoder.decode(ctx, msg.content(), outBuffers);
		}

		public override void channelRegistered(ChannelHandlerContext ctx)
		{
			decoder.channelRegistered(ctx);
		}

		public override void channelUnregistered(ChannelHandlerContext ctx)
		{
			decoder.channelUnregistered(ctx);
		}

		public override void channelActive(ChannelHandlerContext ctx)
		{
			decoder.channelActive(ctx);
		}

		public override void channelInactive(ChannelHandlerContext ctx)
		{
			decoder.channelInactive(ctx);
		}

		public override void channelReadComplete(ChannelHandlerContext ctx)
		{
			decoder.channelReadComplete(ctx);
		}

		public override void userEventTriggered(ChannelHandlerContext ctx, Object evt)
		{
			decoder.userEventTriggered(ctx, evt);
		}

		public override void channelWritabilityChanged(ChannelHandlerContext ctx)
		{
			decoder.channelWritabilityChanged(ctx);
		}

		public override void exceptionCaught(ChannelHandlerContext ctx, Exception cause)
		{
			decoder.exceptionCaught(ctx, cause);
		}

		public override void handlerAdded(ChannelHandlerContext ctx)
		{
			decoder.handlerAdded(ctx);
		}

		public override void handlerRemoved(ChannelHandlerContext ctx)
		{
			decoder.handlerRemoved(ctx);
		}

// 		public override bool isSharable()
// 		{
// 			return decoder.isSharable();
// 		}
	}
}
