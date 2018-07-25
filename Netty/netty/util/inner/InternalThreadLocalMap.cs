using System;
using System.Collections.Generic;

namespace io.netty.util.inner
{
	public class InternalThreadLocalMap : UnpaddedInternalThreadLocalMap
	{
		private static InternalThreadLocalMap instance;

		private InternalThreadLocalMap() { }
		public static InternalThreadLocalMap get()
		{
			if (instance == null)
				instance = new InternalThreadLocalMap();

			return instance;
		}

		public Dictionary<Type, AbstractTypeParameterMatcher> typeParameterMatcherGetCache()
		{
			Dictionary<Type, AbstractTypeParameterMatcher> cache = _typeParameterMatcherGetCache;

			if (cache == null)
			{
				_typeParameterMatcherGetCache = cache = new Dictionary<Type, AbstractTypeParameterMatcher>();
			}

			return cache;
		}

		public Dictionary<Type, Dictionary<String, AbstractTypeParameterMatcher>> typeParameterMatcherFindCache()
		{
			Dictionary<Type, Dictionary<String, AbstractTypeParameterMatcher>> cache = _typeParameterMatcherFindCache;
			if (cache == null)
			{
				_typeParameterMatcherFindCache = cache = new Dictionary<Type, Dictionary<String, AbstractTypeParameterMatcher>>();
			}
			return cache;
		}
	}
}
