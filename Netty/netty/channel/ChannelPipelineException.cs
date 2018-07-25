using System;

namespace io.netty.channel
{
	[Serializable()]
	class ChannelPipelineException : Exception
	{
		public ChannelPipelineException() : base() { }
		public ChannelPipelineException(string message) : base(message) { }
		public ChannelPipelineException(string message, Exception inner) : base(message, inner) { }

		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client. 
		protected ChannelPipelineException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
	}
}
