using System;
using System.Text;

namespace io.netty.channel
{
	public class DefaultChannelId : ChannelId
	{
		private String shortValue = "";
		private String longValue = "";
//		private byte[] data;
//		private static byte[] MACHINE_ID;
//		private static int PROCESS_ID_LEN = 4;
//		private static int PROCESS_ID;
//		private static int SEQUENCE_LEN = 4;
//		private static int TIMESTAMP_LEN = 8;
//		private static int RANDOM_LEN = 4;

		public static DefaultChannelId newInstance()
		{
			return new DefaultChannelId();
		}

		public String asShortText()
		{
			String shortValue = this.shortValue;

			if (shortValue == null)
			{
//				this.shortValue = shortValue = ByteBufUtil.hexDump(data, data.length - RANDOM_LEN, RANDOM_LEN);
			}

			return shortValue;
		}

		public String asLongText()
		{
			String longValue = this.longValue;

			if (longValue == null)
			{
//				this.longValue = longValue = newLongValue();
			}

			return longValue;
		}

// 		private String newLongValue()
// 		{
// 			StringBuilder buf = new StringBuilder(2 * data.Length + 5);
// 			int i = 0;
// 			i = appendHexDumpField(buf, i, MACHINE_ID.Length);
// 			i = appendHexDumpField(buf, i, PROCESS_ID_LEN);
// 			i = appendHexDumpField(buf, i, SEQUENCE_LEN);
// 			i = appendHexDumpField(buf, i, TIMESTAMP_LEN);
// 			i = appendHexDumpField(buf, i, RANDOM_LEN);
// 			Debug.Assert(i == this.data.Length);
// 			return buf.ToString().Substring(0, buf.Length - 1);
// 		}

		private int appendHexDumpField(StringBuilder buf, int i, int length)
		{
//			buf.Append(ByteBuf.HexDump(data, i, length));
			buf.Append('-');
			i += length;
			return i;
		}
	}
}
