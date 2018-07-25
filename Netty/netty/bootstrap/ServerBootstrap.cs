using io.netty.channel;
using NLog;
using System;

namespace io.netty.bootstrap
{
	public class ServerBootstrap : AbstractBootstrap<ServerBootstrap, ServerChannel>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private volatile EventLoopGroup childGroup;
		private volatile ChannelHandler _childHandler;

		public ServerBootstrap() { }

		private ServerBootstrap(ServerBootstrap bootstrap)
			: base(bootstrap)
		{
			childGroup = bootstrap.childGroup;
			_childHandler = bootstrap._childHandler;
		}

		public override ServerBootstrap group(EventLoopGroup _group)
		{
			return group(_group, _group);
		}

		public ServerBootstrap group(EventLoopGroup parentGroup, EventLoopGroup childGroup)
		{
			base.group(parentGroup);
			if (childGroup == null)
			{
				throw new NullReferenceException("childGroup");
			}

			if (this.childGroup != null)
			{
				throw new ArgumentException("childGroup set already");
			}

			this.childGroup = childGroup;
			return this;
		}

		public ServerBootstrap childHandler(ChannelHandler _childHandler)
		{
			if (_childHandler == null)
			{
				throw new NullReferenceException("childHandler");
			}

			this._childHandler = _childHandler;

			return this;
		}

		protected override void init(Channel _channel)
		{
			ChannelPipeline p = _channel.pipeline();

			EventLoopGroup currentChildGroup = childGroup;
			ChannelHandler currentChildHandler = _childHandler;

			p.addLast(new ChannelInitializer<Channel>(channel =>
			{
				ChannelPipeline pipeline = channel.pipeline();
				ChannelHandler _handler = handler();
				if (_handler != null)
				{
					pipeline.addLast(_handler);
				}

				channel.eventLoop().execute(() => pipeline.addLast(new ServerBootstrapAcceptor(channel, currentChildGroup, currentChildHandler)));
			}));
		}

		private class ServerBootstrapAcceptor : ChannelInboundHandlerAdapter
		{
			private EventLoopGroup childGroup;
			private ChannelHandler childHandler;
//			private Action enableAutoReadTask;

			public ServerBootstrapAcceptor(Channel channel, EventLoopGroup childGroup, ChannelHandler childHandler)
			{
				this.childGroup = childGroup;
				this.childHandler = childHandler;

				// Task which is scheduled to re-enable auto-read.
				// It's important to create this Runnable before we try to submit it as otherwise the URLClassLoader may
				// not be able to load the class because of the file limit it already reached.
				//
				// See https://github.com/netty/netty/issues/1328
//				enableAutoReadTask = channel.config().setAutoRead(true);
			}

			public override void channelRead(ChannelHandlerContext ctx, Object msg)
			{
				Channel child = (Channel)msg;

				child.pipeline().addLast(childHandler);
				childGroup.register(child);
			}

			private static void forceClose(Channel child, Exception t)
			{
//				child.unsafe().closeForcibly();
				logger.Warn(t, String.Format("Failed to register an accepted channel: {0}", child.ToString()));
			}

			public override void exceptionCaught(ChannelHandlerContext ctx, Exception cause)
			{
// 				ChannelConfig config = ctx.channel().config();
// 
// 				if (config.isAutoRead())
// 				{
// 					// stop accept new connections for 1 second to allow the channel to recover
// 					// See https://github.com/netty/netty/issues/1328
// 					config.setAutoRead(false);
// 					ctx.channel().eventLoop().schedule(enableAutoReadTask, 1, TimeUnit.SECONDS);
// 				}
// 				// still let the exceptionCaught event flow through the pipeline to give the user
// 				// a chance to do something with it
// 				ctx.fireExceptionCaught(cause);
			}
		}
	}
}
