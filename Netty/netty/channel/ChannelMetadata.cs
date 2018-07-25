using System;

namespace io.netty.channel
{
	public class ChannelMetadata
	{
		private bool _hasDisconnect;
		private int _defaultMaxMessagesPerRead;

		public ChannelMetadata(bool hasDisconnect)
			: this(hasDisconnect, 1)
		{
		}

		public ChannelMetadata(bool _hasDisconnect, int _defaultMaxMessagesPerRead)
		{
			if (_defaultMaxMessagesPerRead <= 0)
			{
				throw new ArgumentException("defaultMaxMessagesPerRead: " + _defaultMaxMessagesPerRead.ToString() + " (expected > 0)");
			}
			this._hasDisconnect = _hasDisconnect;
			this._defaultMaxMessagesPerRead = _defaultMaxMessagesPerRead;
		}

		public bool hasDisconnect()
		{
			return _hasDisconnect;
		}

		public int defaultMaxMessagesPerRead()
		{
			return _defaultMaxMessagesPerRead;
		}
	}
}
