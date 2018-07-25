using System;
using System.Threading;

namespace io.netty.util.concurrent
{
	public class DefaultThreadFactory : ThreadFactory
	{
		public Thread newThread(Action evt)
		{
			ParameterizedThreadStart operation = new ParameterizedThreadStart(obj => evt());
			return new Thread(operation);
		}
	}
}
