using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EudiFramework
{
    [UnityEngine.Scripting.Preserve]
    public static class TypeCacherIsSubclassOf<TCheck, TAgainst>
    {
        public static readonly Type TypeCheck = typeof(TCheck);
        public static readonly Type TypeAgainst = typeof(TAgainst);
        public static readonly bool IsSubclass = TypeCheck.IsSubclassOf(TypeAgainst);
    }
    
    [UnityEngine.Scripting.Preserve]
    public static class TypeCacher<T>
    {
        public static readonly Type Type = typeof(T);
        public static readonly bool IsValueType = TypeCacher<T>.Type.IsValueType;
        public static readonly bool IsBlittable;
        /// <summary>
        /// Return the size of the type. Return -1 if it's not blittable
        /// </summary>
        public static readonly int SizeOf;
        
        static TypeCacher()
        {
            try
            {
                // Class test
                if (default(T) != null)
                {
                    // Non-blittable types cannot allocate pinned handle
                    GCHandle.Alloc(default(T), GCHandleType.Pinned).Free();
                    IsBlittable = true;
                }
            }
            catch
            {
                // ignored
            }

            try
            {
                SizeOf = Marshal.SizeOf(Type);
            }
            catch
            {
                SizeOf = -1;
            }
        }
    }

    public static class TypeCacher
    {
        /// <summary>
        /// generated generic type = Dico[maker][generic];
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Type, Dictionary<Type, Type>> GeneratedGenerics = new Dictionary<Type, Dictionary<Type, Type>>();

        public static Dictionary<Type, bool> GeneratedBoolsIsGeneric = new Dictionary<Type, bool>();
        public static Dictionary<Type, Dictionary<Type, bool>> GeneratedBoolsEqualGenericDef = new Dictionary<Type, Dictionary<Type, bool>>();
        public static Dictionary<long, Type> GeneratedHandlesTypeRelation = new Dictionary<long, Type>();
        
        public static Type MakeGeneric(Type maker, Type generic)
        {
            Dictionary<Type, Type> _dicoGenerated;
            if (!GeneratedGenerics.ContainsKey(maker))
                _dicoGenerated = GeneratedGenerics[maker] = new Dictionary<Type, Type>();
            else
                _dicoGenerated = GeneratedGenerics[maker];

            if (_dicoGenerated.ContainsKey(generic))
                return _dicoGenerated[generic];

            return _dicoGenerated[generic] = maker.MakeGenericType(generic);
        }

        public static bool IsGeneric(Type type)
        {
            if (GeneratedBoolsIsGeneric.ContainsKey(type))
                return GeneratedBoolsIsGeneric[type];

            return GeneratedBoolsIsGeneric[type] = type.IsGenericType;
        }

        public static bool IsEqualToGenericDefinition(Type type, Type genericDefinition)
        {
            Dictionary<Type, bool> _dicoGenerated;
            if (!GeneratedBoolsEqualGenericDef.ContainsKey(genericDefinition))
                _dicoGenerated = GeneratedBoolsEqualGenericDef[genericDefinition] = new Dictionary<Type, bool>();
            else
                _dicoGenerated = GeneratedBoolsEqualGenericDef[genericDefinition];

            if (_dicoGenerated.ContainsKey(type))
                return _dicoGenerated[type];

            return _dicoGenerated[type] = type.GetGenericTypeDefinition() == genericDefinition;
        }

        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            var handleInt = handle.Value.ToInt64();
            
            if (GeneratedHandlesTypeRelation.ContainsKey(handleInt))
                return GeneratedHandlesTypeRelation[handleInt] ?? (GeneratedHandlesTypeRelation[handleInt] = Type.GetTypeFromHandle(handle));

            return GeneratedHandlesTypeRelation[handleInt] = Type.GetTypeFromHandle(handle);
        }
    }
}