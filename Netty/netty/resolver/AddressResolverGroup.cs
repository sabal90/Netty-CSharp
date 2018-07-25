using io.netty.util.concurrent;
using System;
using System.Collections.Generic;
using System.Net;

namespace io.netty.resolver
{
	public abstract class AddressResolverGroup<T> where T : SocketAddress
	{
		private Dictionary<EventExecutor, AddressResolver<T>> resolvers = new Dictionary<EventExecutor, AddressResolver<T>>(32);

		public AddressResolver<T> getResolver(EventExecutor executor)
		{
			if (executor == null)
			{
				throw new NullReferenceException("executor");
			}

// 			if (executor.isShuttingDown())
// 			{
// 				throw new ArgumentException("executor not accepting a task");
// 			}

			AddressResolver<T> r;
			lock (resolvers)
			{
				resolvers.TryGetValue(executor, out r);
				if (r == null)
				{
					AddressResolver<T> _newResolver;
					try
					{
						_newResolver = newResolver(executor);
					}
					catch (Exception e)
					{
						throw new ArgumentException("failed to create a new resolver", e);
					}

					resolvers.Add(executor, _newResolver);

					r = _newResolver;
				}
			}

			return r;
		}

		protected abstract AddressResolver<T> newResolver(EventExecutor executor);
	}
}
