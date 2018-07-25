using io.netty.channel;
using io.netty.buffer;
using System;
using System.Collections.Generic;

namespace io.netty.handler.codec
{
	public abstract class ByteToMessageDecoder : ChannelInboundHandlerAdapter
	{
		IList<Object> outBuffers;
		ByteBuf inBuffer;

		protected ByteToMessageDecoder()
		{
			inBuffer = ByteBuf.Allocate(1024);
			outBuffers = new List<Object>();
		}

		public override void channelRead(ChannelHandlerContext ctx, Object msg)
		{
			inBuffer.Append((byte[])msg);
			decode(ctx, inBuffer, outBuffers);

			if (outBuffers.Count == 0)
				return;

			for (int i = 0; i < outBuffers.Count; i++)
				ctx.fireChannelRead(outBuffers[i]);

			outBuffers.Clear();
		}

		protected abstract void decode(ChannelHandlerContext ctx, ByteBuf inBuffer, IList<Object> outBuffers);
	}
}
