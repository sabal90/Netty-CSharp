using System;
using System.Collections.Generic;

namespace io.netty.util.inner
{
	public abstract class AbstractTypeParameterMatcher
	{
		protected static AbstractTypeParameterMatcher NOOP;

		public static AbstractTypeParameterMatcher get(Type parameterType)
		{
			Dictionary<Type, AbstractTypeParameterMatcher> getCache = InternalThreadLocalMap.get().typeParameterMatcherGetCache();
			AbstractTypeParameterMatcher matcher;
			getCache.TryGetValue(parameterType, out matcher);

			if (matcher == null)
			{
				if (parameterType == typeof(Object))
				{
					matcher = NOOP;
				}
				else
				{
					matcher = new ReflectiveMatcher(parameterType);
				}
				getCache.Add(parameterType, matcher);
			}

			return matcher;
		}

		public static AbstractTypeParameterMatcher find(Object obj, Type parametrizedSuperclass, String typeParamName)
		{
			Dictionary<Type, Dictionary<String, AbstractTypeParameterMatcher>> findCache = InternalThreadLocalMap.get().typeParameterMatcherFindCache();
			Type thisClass = obj.GetType();

			Dictionary<String, AbstractTypeParameterMatcher> map;
			findCache.TryGetValue(thisClass, out map);

			if (map == null)
			{
				map = new Dictionary<String, AbstractTypeParameterMatcher>();
				findCache.Add(thisClass, map);
			}

			AbstractTypeParameterMatcher matcher;
			map.TryGetValue(typeParamName, out matcher);

			if (matcher == null)
			{
				matcher = get(find0(obj, parametrizedSuperclass, typeParamName));
				map.Add(typeParamName, matcher);
			}

			return matcher;
		}

		private static Type find0(Object obj, Type parametrizedSuperclass, String typeParamName)
		{
			Type thisClass = obj.GetType();
			Type currentClass = thisClass;

			for (; ; )
			{
				if (currentClass.BaseType.GetGenericTypeDefinition() == parametrizedSuperclass)
				{
					int typeParamIndex = -1;
					Type[] typeParams = currentClass.BaseType.GetGenericTypeDefinition().GetGenericArguments();

					for (int i = 0; i < typeParams.Length; i++)
					{
						if (typeParamName.Equals(typeParams[i].Name))
						{
							typeParamIndex = i;
							break;
						}
					}

					if (typeParamIndex < 0)
					{
						throw new ArgumentException("unknown type parameter '" + typeParamName + "': " + parametrizedSuperclass);
					}

					Type genericSuperType = currentClass.BaseType;
// 					if (genericSuperType.IsGenericTypeDefinition)
// 					{
// 						return typeof(Object);
// 					}

					Type[] actualTypeParams = genericSuperType.GetGenericArguments();

					Type actualTypeParam = actualTypeParams[typeParamIndex];

// 					if (actualTypeParam is ParameterizedType)
// 					{
// 						actualTypeParam = ((ParameterizedType) actualTypeParam).getRawType();
// 					}

					if (actualTypeParam.IsClass)
					{
						return actualTypeParam;
					}

/*					if (actualTypeParam instanceof GenericArrayType) {
						Type componentType = ((GenericArrayType) actualTypeParam).getGenericComponentType();
						if (componentType instanceof ParameterizedType) {
							componentType = ((ParameterizedType) componentType).getRawType();
						}
						if (componentType instanceof Class) {
							return Array.newInstance((Type) componentType, 0).GetType();
						}
					}
					if (actualTypeParam instanceof TypeVariable) {
						// Resolved type parameter points to another type parameter.
						TypeVariable<?> v = (TypeVariable<?>) actualTypeParam;
						currentClass = thisClass;
						if (!(v.getGenericDeclaration() instanceof Class)) {
							return typeof(Object);
						}

						parametrizedSuperclass = (Class<?>) v.getGenericDeclaration();
						typeParamName = v.getName();
						if (parametrizedSuperclass.isAssignableFrom(thisClass)) {
							continue;
						} else {
							return typeof(Object);
						}
					}*/

					return fail(thisClass, typeParamName);
				}

				currentClass = currentClass.BaseType;

				if (currentClass == null)
				{
					return fail(thisClass, typeParamName);
				}
			}
		}

		private static Type fail(Type type, String typeParamName)
		{
			throw new ArgumentException("cannot determine the type of the type parameter '" + typeParamName + "': " + type);
		}

		public abstract bool match(Object msg);

		private class ReflectiveMatcher : AbstractTypeParameterMatcher
		{
			private Type type;

			public ReflectiveMatcher(Type type)
			{
				this.type = type;
			}

			public override bool match(Object msg)
			{
				return type.IsAssignableFrom(msg.GetType());
			}
		}

		public AbstractTypeParameterMatcher() { }
	}
}
