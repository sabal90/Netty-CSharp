using System;
using System.Threading;

namespace io.netty.util.concurrent
{
	public interface ThreadFactory
	{
		Thread newThread(Action evt);
	}
}
