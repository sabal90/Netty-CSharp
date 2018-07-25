using System;

namespace io.netty.handler.codec
{
	[Serializable()]
	class DecoderException : CodecException
	{
		public DecoderException() : base() { }
		public DecoderException(string message) : base(message) { }
		public DecoderException(string message, Exception inner) : base(message, inner) { }

		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client. 
		protected DecoderException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
	}
}
