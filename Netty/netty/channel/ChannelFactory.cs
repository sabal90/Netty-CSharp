
namespace io.netty.channel
{
	public interface ChannelFactory<T> where T : Channel
	{
		T newChannel();
	}
}
