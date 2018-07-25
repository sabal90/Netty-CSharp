using System;
using System.Diagnostics;

namespace io.netty.channel
{
	public class ChannelInitializer<C> : AbstractChannelInitializer<C>
		where C : Channel
	{
		readonly Action<C> initializationAction;

		public ChannelInitializer(Action<C> initializationAction)
		{
			Debug.Assert(initializationAction != null);

			this.initializationAction = initializationAction;
		}

		protected override void initChannel(C _channel)
		{
			this.initializationAction(_channel);
		}
	}
}
