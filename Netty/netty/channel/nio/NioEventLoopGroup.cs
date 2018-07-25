using io.netty.nio;
using io.netty.util.concurrent;
using System;

namespace io.netty.channel.nio
{
	public class NioEventLoopGroup : MultithreadEventLoopGroup
	{
		public NioEventLoopGroup() : this(0) { }
		public NioEventLoopGroup(int nThreads) : this(nThreads, null) { }
		public NioEventLoopGroup(int nThreads, Executor executor) : this(nThreads, executor, SelectorProvider.provider()) { }
		public NioEventLoopGroup(int nThreads, Executor executor, SelectorProvider provider) : base(nThreads, executor, provider) { }
		protected override EventExecutor newChild(Executor executor, params Object[] args)
		{
			return new NioEventLoop(this, executor, (SelectorProvider)args[0]);
		}
	}
}
