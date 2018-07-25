
namespace io.netty.util.concurrent
{
	public abstract class AbstractScheduledEventExecutor : AbstractEventExecutor
	{
		protected AbstractScheduledEventExecutor(EventExecutorGroup parent) : base(parent) { }
	}
}
