using System;

namespace io.netty.channel
{
	[Serializable()]
	class ClosedChannelException : Exception
	{
		public ClosedChannelException() : base() { }
		public ClosedChannelException(string message) : base(message) { }
		public ClosedChannelException(string message, Exception inner) : base(message, inner) { }

		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client. 
		protected ClosedChannelException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
	}
}
