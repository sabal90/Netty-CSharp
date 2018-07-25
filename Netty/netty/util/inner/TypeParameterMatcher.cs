using System;
using System.Diagnostics;

namespace io.netty.util.inner
{
	public class TypeParameterMatcher : AbstractTypeParameterMatcher
	{
		Func<Object, bool> _match;

		public TypeParameterMatcher()
		{
			NOOP = new TypeParameterMatcher((msg) => { return true; });
		}

		private TypeParameterMatcher(Func<Object, bool> _match)
		{
			Debug.Assert(_match != null);
			this._match = _match;
		}

		public override bool match(Object msg)
		{
			return _match(msg);
		}
	}
}
