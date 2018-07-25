using System;
using System.Collections.Generic;

namespace io.netty.util.inner
{
	public class UnpaddedInternalThreadLocalMap
	{
		protected Dictionary<Type, AbstractTypeParameterMatcher> _typeParameterMatcherGetCache;
		protected Dictionary<Type, Dictionary<String, AbstractTypeParameterMatcher>> _typeParameterMatcherFindCache;
	}
}
