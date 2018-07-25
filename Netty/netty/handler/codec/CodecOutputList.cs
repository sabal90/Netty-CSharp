using System;
using System.Collections.Generic;

namespace io.netty.handler.codec
{
	public class CodecOutputList : List<Object>
	{
//		private static Recycler<CodecOutputList> RECYCLER = new Recycler<CodecOutputList>();

		public static CodecOutputList newInstance()
		{
//			return RECYCLER.get();
			return null;
		}
	}
}
