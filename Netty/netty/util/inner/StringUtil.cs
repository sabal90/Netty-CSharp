using System;

namespace io.netty.util.inner
{
    public class StringUtil
    {
		public static String simpleClassName(Object o)
		{
			if (o == null)
			{
				return "null_object";
			}
			else
			{
				return o.GetType().Name;
			}
		}
    }
}
