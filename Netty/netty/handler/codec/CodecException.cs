using System;

namespace io.netty.handler.codec
{
	[Serializable()]
	public class CodecException : Exception
	{
		public CodecException() : base() { }
		public CodecException(string message) : base(message) { }
		public CodecException(string message, Exception inner) : base(message, inner) { }

		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client. 
		protected CodecException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
	}
}
