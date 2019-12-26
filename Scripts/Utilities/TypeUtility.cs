using System;
using System.Collections.Generic;
using System.Text;

namespace StormiumTeam.Shared
{
	public static class TypeUtility
	{
		public static string SpecifiedTypeName(Type type)
		{
			return SpecifiedTypeName(type, new Queue<Type>(type.GetGenericArguments()));
		}

		private static string SpecifiedTypeName(Type type, Queue<Type> args)
		{
			var name = type.Name;
			if (type.IsGenericParameter) return name;

			if (type.IsNested) name = $"{SpecifiedTypeName(type.DeclaringType, args)}.{name}";

			if (type.IsGenericType)
			{
				var tickIndex = name.IndexOf('`');
				if (tickIndex > -1)
					name = name.Remove(tickIndex);
				var genericTypes = type.GetGenericArguments();

				var genericTypeNames = new StringBuilder();
				for (var i = 0; i < genericTypes.Length && args.Count > 0; i++)
				{
					if (i != 0)
						genericTypeNames.Append(", ");
					genericTypeNames.Append(SpecifiedTypeName(args.Dequeue()));
				}

				if (genericTypeNames.Length > 0) name = $"{name}<{genericTypeNames}>";
			}

			return name;
		}
	}
}