using System.Runtime.CompilerServices;
using UnityEngine;

namespace package.stormiumteam.shared
{
    public static class StMath
    {
        public static (uint, uint) ULongToDoubleUInt(ulong l1) {
            var a1 = (uint)(l1 & uint.MaxValue);
            var a2 = (uint)(l1 >> 32);
            return (a1, a2);
        }

        public static ulong DoubleUIntToULong(uint i1, uint i2)
        {
            return ((ulong) i2 << 32) | i1;
        }
        
        public static ulong DoubleIntToULong(int i1, int i2)
        {
            var ui1 = IntToUInt(i1);
            var ui2 = IntToUInt(i2);
            
            return ((ulong) ui2 << 32) | ui1;
        }
        
        public static (int, int) LongToDoubleInt(long l1) {
            var a1 = (int)(l1 & uint.MaxValue);
            var a2 = (int)(l1 >> 32);
            return (a1, a2);
        }

        public static long DoubleIntToLong(int i1, int i2)
        {
            long b = i2;
            b = b << 32;
            b = b | (uint)i1;
            return b;
        }
        
        public static long ULongToLong(ulong ulongValue)
        {
            return unchecked((long)ulongValue + long.MinValue);
        }

        public static ulong LongToULong(long longValue)
        {
            return unchecked((ulong)(longValue - long.MinValue));
        }
        
        public static int UIntToInt(uint ulongValue)
        {
            return unchecked((int)ulongValue + int.MinValue);
        }

        public static uint IntToUInt(int longValue)
        {
            return unchecked((uint)(longValue - int.MinValue));
        }
        
        public static float Distance(float l, float r)
        {
            return Mathf.Abs(l - r);
        }
        
        /// <summary>
        /// Increase origin value to destination value by a step value
        /// </summary>
        /// <param name="o">The origin value</param>
        /// <param name="d">The destination value</param>
        /// <param name="s">The step value</param>
        /// <returns>The new stepped result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Step(float o, float d, float s)
        {
            var r = o;
            r = d > o 
                ? r + s     // r += s
                : r - s;   // r -= s

            if 
            (
                // d = 5, o = 5.5, t = 1, r => o - t = 4.5
                // d < o and d > r
                // so r = d
                (d <= o && d >= r)
                // d = 8, o = 7.5, t = 1, r => o + t = 8.5
                // d > o and d < r
                // so r = d
                || (d >= o && d <= r)
            )
            {
                r = d;
            }
            
            return r;
        }

        /// <summary>
        /// Increase origin value to destination value by a step value
        /// </summary>
        /// <param name="o">The origin value</param>
        /// <param name="d">The destination value</param>
        /// <param name="s">The step value</param>
        /// <returns>The new stepped result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Step(Vector3 o, Vector3 d, float s)
        {
            return new Vector3
            (
                Step(o.x, d.x, s),
                Step(o.y, d.y, s),
                Step(o.z, d.z, s)
            );
        }
        
        /// <summary>
        /// Increase origin value to destination value by a step value
        /// </summary>
        /// <param name="o">The origin value</param>
        /// <param name="d">The destination value</param>
        /// <param name="s">The step value</param>
        /// <returns>The new stepped result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Step(Vector3 o, Vector3 d, Vector3 s)
        {
            return new Vector3
            (
                Step(o.x, d.x, s.x),
                Step(o.y, d.y, s.y),
                Step(o.z, d.z, s.z)
            );
        }

        /// <summary>
        /// Translate the origin to the destination by a heading method
        /// </summary>
        /// <param name="o">The origin value</param>
        /// <param name="d">The destination value</param>
        /// <param name="speed">The speed value</param>
        /// <returns>The new headed result</returns>
        public static Vector3 Heading(Vector3 o, Vector3 d, float speed)
        {
            return Vector3.Lerp(o, d, speed / Vector3.Distance(o, d));
        }
    }
}