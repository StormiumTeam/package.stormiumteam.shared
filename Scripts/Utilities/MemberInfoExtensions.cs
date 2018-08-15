using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace package.stormiumteam.shared
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Returns the first found custom attribute of type T on this member
        /// Returns null if none was found
        /// </summary>
        public static T GetAttribute<T>(this MemberInfo member, bool inherit) where T : Attribute
        {
            T[] array = member.GetAttributes<T>(inherit).ToArray<T>();
            if (array != null && array.Length != 0)
                return array[0];
            return default (T);
        }

        /// <summary>
        /// Returns the first found non-inherited custom attribute of type T on this member
        /// Returns null if none was found
        /// </summary>
        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetAttribute<T>(false);
        }

        /// <summary>Gets all attributes of the specified generic type.</summary>
        /// <param name="member">The member.</param>
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetAttributes<T>(false);
        }

        /// <summary>Gets all attributes of the specified generic type.</summary>
        /// <param name="member">The member.</param>
        /// <param name="inherit">If true, specifies to also search the ancestors of element for custom attributes.</param>
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member, bool inherit) where T : Attribute
        {
            return member.GetCustomAttributes(typeof (T), inherit).Cast<T>();
        }
    }
}