using System;

namespace io.netty.nio
{
	public class IllegalSelectorException : Exception
	{
		public IllegalSelectorException() : base() { }
		public IllegalSelectorException(string message) : base(message) { }
		public IllegalSelectorException(string message, Exception inner) : base(message, inner) { }

		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client. 
		protected IllegalSelectorException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
	}
}
