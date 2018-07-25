using System;

namespace io.netty.channel
{
	public class ChannelException : Exception
	{
		public ChannelException() : base() { }
		public ChannelException(string message) : base(message) { }
		public ChannelException(string message, Exception inner) : base(message, inner) { }

		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client. 
		protected ChannelException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
	}
}
