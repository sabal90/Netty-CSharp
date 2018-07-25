
namespace io.netty.util.concurrent
{
	public interface EventExecutorChooser
	{
		EventExecutor next();
	}

	public interface EventExecutorChooserFactory
	{
		EventExecutorChooser newChooser(EventExecutor[] executors);
	}
}
