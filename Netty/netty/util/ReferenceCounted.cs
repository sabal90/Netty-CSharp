using System;

namespace io.netty.util
{
	public interface ReferenceCounted
	{
		/**
		 * Returns the reference count of this object.  If {@code 0}, it means this object has been deallocated.
		 */
		int refCnt();

		/**
		 * Increases the reference count by {@code 1}.
		 */
		ReferenceCounted retain();

		/**
		 * Increases the reference count by the specified {@code increment}.
		 */
		ReferenceCounted retain(int increment);

		/**
		 * Records the current access location of this object for debugging purposes.
		 * If this object is determined to be leaked, the information recorded by this operation will be provided to you
		 * via {@link ResourceLeakDetector}.  This method is a shortcut to {@link #touch(Object) touch(null)}.
		 */
		ReferenceCounted touch();

		/**
		 * Records the current access location of this object with an additional arbitrary information for debugging
		 * purposes.  If this object is determined to be leaked, the information recorded by this operation will be
		 * provided to you via {@link ResourceLeakDetector}.
		 */
		ReferenceCounted touch(Object hint);

		/**
		 * Decreases the reference count by {@code 1} and deallocates this object if the reference count reaches at
		 * {@code 0}.
		 *
		 * @return {@code true} if and only if the reference count became {@code 0} and this object has been deallocated
		 */
		bool release();

		/**
		 * Decreases the reference count by the specified {@code decrement} and deallocates this object if the reference
		 * count reaches at {@code 0}.
		 *
		 * @return {@code true} if and only if the reference count became {@code 0} and this object has been deallocated
		 */
		bool release(int decrement);
	}
}
