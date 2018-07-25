using io.netty.util;

namespace io.netty.buffer
{
	public interface ByteBufHolder : ReferenceCounted
	{
		/**
		 * Return the data which is held by this {@link ByteBufHolder}.
		 */
		ByteBuf content();

		/**
		 * Creates a deep copy of this {@link ByteBufHolder}.
		 */
		ByteBufHolder copy();

		/**
		 * Duplicates this {@link ByteBufHolder}. Be aware that this will not automatically call {@link #retain()}.
		 */
		ByteBufHolder duplicate();

		/**
		 * Duplicates this {@link ByteBufHolder}. This method returns a retained duplicate unlike {@link #duplicate()}.
		 *
		 * @see ByteBuf#retainedDuplicate()
		 */
		ByteBufHolder retainedDuplicate();

		/**
		 * Returns a new {@link ByteBufHolder} which contains the specified {@code content}.
		 */
		ByteBufHolder replace(ByteBuf content);
	}
}
