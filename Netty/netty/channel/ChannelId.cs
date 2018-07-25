using System;

namespace io.netty.channel
{
	public interface ChannelId
	{
		String asShortText();
		String asLongText();
	}
}
