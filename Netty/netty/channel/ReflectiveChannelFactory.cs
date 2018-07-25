using System;

namespace io.netty.channel
{
	class ReflectiveChannelFactory<T> : ChannelFactory<T> where T : Channel
	{
		private Type clazz;

		public ReflectiveChannelFactory(Type clazz)
		{
			if (clazz == null)
			{
				throw new NullReferenceException("clazz");
			}
			this.clazz = clazz;
		}

		public T newChannel()
		{
			try
			{
				return (T)Activator.CreateInstance(clazz);
			}
			catch (Exception t)
			{
				throw new Exception("Unable to create Channel from class " + clazz, t);
			}
		}

		public String toString()
		{
			return clazz.Name + ".class";
		}
	}
}
