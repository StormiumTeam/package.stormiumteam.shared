using System;
using System.Runtime.Remoting.Messaging;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Scripts.Utilities
{
    public static unsafe class FastStructComparer
    {
        public static bool Equals(byte* s1, int size1, byte* s2, int size2)
        {
            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (size1 < 0 || size2 < 0)
                throw new ArgumentException("Out of bounds.");
            if ((IntPtr)s1 == IntPtr.Zero || (IntPtr)s2 == IntPtr.Zero)
                throw new ArgumentException("Null pointers");
            #endif
            
            // If both of the size are equal to 0, return true. (there is no interest to compare them)
            if (size1 == 0 && size2 == 0)
                return true;
            
            // If the size don't match, return false.
            if (size1 != size2)
                return false;

            // Iterate over the bytes of each structures
            for (int index = 0; index != size1; index++)
            {
                if (s1[index] != s2[index])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool FastEquals<T1, T2>(this T1 s1, ref T2 s2)
            where T1 : struct
            where T2 : struct
        {            
            var size1 = UnsafeUtility.SizeOf<T1>();
            var size2 = UnsafeUtility.SizeOf<T2>();

            return Equals((byte*) UnsafeUtility.AddressOf(ref s1), size1, (byte*) UnsafeUtility.AddressOf(ref s2), size2);
        }
    }
}