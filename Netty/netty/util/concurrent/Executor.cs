using System;

namespace io.netty.util.concurrent
{
	public interface Executor
	{
		void execute(Action action);
	}
}
