using io.netty.buffer;
using io.netty.channel;
using System;
using System.Collections.Generic;

namespace io.netty.handler.codec.bytes
{
	public class ByteArrayDecoder : MessageToMessageDecoder<ByteBuf>
	{
		public override void decode(ChannelHandlerContext ctx, ByteBuf msg, List<Object> outBuffers)
		{
			outBuffers.Add(msg.GetBytes());
		}
	}
}
