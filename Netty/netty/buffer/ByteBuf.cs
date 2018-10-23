using System;
using System.Text;

namespace io.netty.buffer
{
	public class ByteBuf
	{
		private int position;
		private byte[] pbyBuffer;
		private bool pooled;
		ByteOrder bo;

		private ByteBuf(bool pooled = true)
		{
			bo = ByteOrder.nativeOrder();
			this.pooled = pooled;
			position = -1;
		}

		public static ByteBuf Allocate(int _maxSize)
		{
			ByteBuf buff = new ByteBuf();
			buff.pbyBuffer = new byte[_maxSize];

			return buff;
		}

		public static ByteBuf Wrap(byte[] array, int offset, int length)
		{
			ByteBuf buff = new ByteBuf();
			buff.pbyBuffer = new byte[length - offset];
			Array.Copy(array, offset, buff.pbyBuffer, 0, length - offset);
			buff.position += length;

			return buff;
		}

		public static ByteBuf Wrap(byte[] array)
		{
			return Wrap(array, 0, array.Length);
		}

		public static ByteBuf Wrap(String msg)
		{
			byte[] array = Encoding.Default.GetBytes(msg);

			return Wrap(array);
		}

		public static ByteBuf Wrap(int value, ByteOrder bo)
		{
			ByteBuf buff = ByteBuf.Allocate(sizeof(int)).Order(bo);
			buff.PutInt(value);

			return buff;
		}

		public ByteBuf copy()
		{
			return ByteBuf.Wrap(pbyBuffer);
		}

		public ByteBuf duplicate()
		{
			return new ByteBuf(false).setBuffer(pbyBuffer).setPosition(position);
		}

		public ByteBuf retainedDuplicate()
		{
			return null;
		}

		private ByteBuf setBuffer(byte[] buffer)
		{
			pbyBuffer = buffer;
			return this;
		}

		private ByteBuf setPosition(int position)
		{
			this.position = position;
			return this;
		}

		public bool Append(byte[] _pbyBuffer)
		{
			return Append(_pbyBuffer, _pbyBuffer.Length);
		}

		public bool Append(byte[] _pbyBuffer, int length)
		{
			if (length <= 0)
				return false;

			int remainingSize = GetRemainingSize();

			if (remainingSize < length)
			{
				Array.Resize(ref pbyBuffer, pbyBuffer.Length + length - remainingSize);
// 				Console.WriteLine("=====================================");
// 				Console.WriteLine("new expanded data buffer size : " + pbyBuffer.Length);
// 				Console.WriteLine("=====================================");
			}

			// Copy the data
			Array.Copy(_pbyBuffer, 0, pbyBuffer, position + 1, length);

			// Update the size of the buffer
			position += length;

			return true;
		}

		public ByteBuf Order(ByteOrder bo)
		{
			this.bo = bo;

			return this;
		}

		public void Pop(int _dataSize)
		{
			if (position == -1)
				return;

			if (_dataSize >= position + 1)
			{
				Reset();
				return;
			}

			Array.Copy(pbyBuffer, _dataSize, pbyBuffer, 0, pbyBuffer.Length - _dataSize);
			position -= _dataSize;
		}

		public int Size()
		{
			return position + 1;
		}

		public int MaxSize()
		{
			return pbyBuffer.Length;
		}

		public ByteBuf Put(byte value)
		{
			pbyBuffer[++position] = value;

			return this;
		}

		public ByteBuf Put(byte[] src, int offset, int length)
		{
			if (src == null)
				return this;

			Array.Copy(src, offset, pbyBuffer, position + 1, length);
			position += length;

			return this;
		}

		public ByteBuf Put(byte[] src)
		{
			if (src == null)
				return this;

			return Put(src, 0, src.Length);
		}

		public ByteBuf PutShort(short value)
		{
			PutValue(BitConverter.GetBytes(value));
			position += sizeof(short);
			return this;
		}

		public ByteBuf PutInt(int value)
		{
			PutValue(BitConverter.GetBytes(value));
			position += sizeof(Int32);
			return this;
		}

		public ByteBuf PutDouble(double value)
		{
			PutValue(BitConverter.GetBytes(value));
			position += sizeof(double);
			return this;
		}

		public ByteBuf PutFloat(float value)
		{
			PutValue(BitConverter.GetBytes(value));
			position += sizeof(float);
			return this;
		}

		public ByteBuf PutLong(long value)
		{
			PutValue(BitConverter.GetBytes(value));
			position += sizeof(long);
			return this;
		}

		public ByteBuf PutString(String value)
		{
			PutValue(Encoding.Default.GetBytes(value));
			position += value.Length;
			return this;
		}

		private ByteBuf PutValue(byte[] array)
		{
			if (array == null)
				return this;

			if (ByteOrder.nativeOrder() != bo)
				Array.Reverse(array);

			Array.Copy(array, 0, pbyBuffer, position + 1, array.Length);

			return this;
		}

		public String GetString(int offset, int length)
		{
			return Encoding.Default.GetString(pbyBuffer, offset, length);
		}

		public String GetString()
		{
			return GetString(0, position + 1);
		}

		public byte[] GetBytes(int beginIndex, int endIndex)
		{
			byte[] result = new byte[endIndex - beginIndex];
			Array.Copy(pbyBuffer, beginIndex, result, 0, endIndex - beginIndex);

			return result;
		}

		public byte[] GetBytes()
		{
			return GetBytes(0, Size());
		}

		public short GetShort(int offset = 0)
		{
			byte[] tmp = new byte[sizeof(short)];
			Array.Copy(pbyBuffer, offset, tmp, 0, tmp.Length);

			if (ByteOrder.nativeOrder() != bo)
				Array.Reverse(tmp);

			return BitConverter.ToInt16(tmp, 0);
		}

		public int GetInt(int offset = 0)
		{
			byte[] tmp = new byte[sizeof(Int32)];
			Array.Copy(pbyBuffer, offset, tmp, 0, tmp.Length);

			if (ByteOrder.nativeOrder() != bo)
				Array.Reverse(tmp);

			return BitConverter.ToInt32(tmp, 0);
		}

		public double GetDouble(int offset = 0)
		{
			byte[] tmp = new byte[sizeof(double)];
			Array.Copy(pbyBuffer, offset, tmp, 0, tmp.Length);

			if (ByteOrder.nativeOrder() != bo)
				Array.Reverse(tmp);

			return BitConverter.ToDouble(tmp, 0);
		}

		public float GetFloat(int offset = 0)
		{
			byte[] tmp = new byte[sizeof(double)];
			Array.Copy(pbyBuffer, offset, tmp, 0, tmp.Length);

			if (ByteOrder.nativeOrder() != bo)
				Array.Reverse(tmp);

			return BitConverter.ToSingle(tmp, 0);
		}

		public long GetLong(int offset = 0)
		{
			byte[] tmp = new byte[sizeof(long)];
			Array.Copy(pbyBuffer, offset, tmp, 0, tmp.Length);

			if (ByteOrder.nativeOrder() != bo)
				Array.Reverse(tmp);

			return BitConverter.ToInt64(tmp, 0);
		}

		public String GetString(int offset = 0)
		{
			byte[] tmp = new byte[sizeof(long)];
			Array.Copy(pbyBuffer, offset, tmp, 0, tmp.Length);

			if (ByteOrder.nativeOrder() != bo)
				Array.Reverse(tmp);

			return Encoding.Default.GetString(tmp);
		}

		private int GetRemainingSize()
		{
			return pbyBuffer.Length - position - 1;
		}

		public byte GetByte()
		{
			return GetByte(0);
		}

		public byte GetByte(int offset)
		{
			if (position < offset)
				throw new IndexOutOfRangeException("out of index");

			return pbyBuffer[offset];
		}

		public void Reset()
		{
			position = -1;
		}
	}
}
