using io.netty.buffer;
using io.netty.nio;
using System;
using System.Net;

namespace io.netty.channel.nio
{
	public abstract class AbstractNioChannel : AbstractChannel
	{
		private SelectableChannel ch;
		private volatile SelectionKey _selectionKey = null;

		protected AbstractNioChannel(Channel parent, SelectableChannel ch) : base(parent)
		{
			this.ch = ch;
        }

		public override bool isOpen()
		{
			return true;
		}

		public new NioUnsafe getUnsafe()
		{
			return (NioUnsafe)base.getUnsafe();
		}

		protected new SelectableChannel windowsChannel()
		{
			return ch;
		}

		public new NioEventLoop eventLoop()
		{
			return (NioEventLoop)base.eventLoop();
		}

		protected new SelectionKey selectionKey()
		{
			if (_selectionKey == null)
				throw new Exception("selectionKey is null");

			return _selectionKey;
		}

		public interface NioUnsafe : Unsafe
		{
			SelectableChannel ch();
			void finishConnect();
			void read1();
			void forceFlush();
		}

		protected abstract class AbstractNioUnsafe : AbstractUnsafe, NioUnsafe
		{
			protected AbstractNioUnsafe(AbstractChannel channel) : base(channel) { }

			public SelectableChannel ch()
			{
				return channel.windowsChannel();
			}

			public new void connect(SocketAddress remoteAddress, SocketAddress localAddress)
			{
				channel.doConnect(remoteAddress, localAddress);
			}

			public void finishConnect() { }
			public void read1() { }
			public void forceFlush() { }
		}

		protected ByteBuf newDirectBuffer(ByteBuf buf)
		{
// 			int readableBytes = buf.readableBytes();
// 			if (readableBytes == 0)
// 			{
// 				ReferenceCountUtil.safeRelease(buf);
// 				return Unpooled.EMPTY_BUFFER;
// 			}
// 
// 			ByteBufAllocator alloc = alloc();
// 			if (alloc.isDirectBufferPooled())
// 			{
// 				ByteBuf directBuf = alloc.directBuffer(readableBytes);
// 				directBuf.writeBytes(buf, buf.readerIndex(), readableBytes);
// 				ReferenceCountUtil.safeRelease(buf);
// 				return directBuf;
// 			}
// 
// 			ByteBuf directBuf = ByteBufUtil.threadLocalDirectBuffer();
// 			if (directBuf != null)
// 			{
// 				directBuf.writeBytes(buf, buf.readerIndex(), readableBytes);
// 				ReferenceCountUtil.safeRelease(buf);
// 				return directBuf;
// 			}
// 
// 			// Allocating and deallocating an unpooled direct buffer is very expensive; give up.
			return buf;
		}

		protected override AbstractUnsafe newUnsafe()
		{
			throw new NotImplementedException();
		}

		protected override bool isCompatible(EventLoop loop)
		{
			throw new NotImplementedException();
		}

		protected override void doClose()
		{
			throw new NotImplementedException();
		}

		protected override void doBind(SocketAddress localAddress)
		{
			throw new NotImplementedException();
		}

		public override void doWrite(ByteBuf msg)
		{
			throw new NotImplementedException();
		}

		public override void doConnect(SocketAddress remoteAddress, SocketAddress localAddress)
		{
			throw new NotImplementedException();
		}

		public override void doDisconnect()
		{
			throw new NotImplementedException();
		}
	}
}
