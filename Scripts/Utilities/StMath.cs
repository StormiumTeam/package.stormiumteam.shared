using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace package.stormiumteam.shared
{
    [StructLayout(LayoutKind.Explicit)]
    public struct LongIntUnion
    {
        [FieldOffset(0)]
        public long LongValue;

        [FieldOffset(0)]
        public int Int0Value;

        [FieldOffset(sizeof(int))]
        public int Int1Value;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct ULongUIntUnion
    {
        [FieldOffset(0)]
        public ulong LongValue;

        [FieldOffset(0)]
        public uint Int0Value;

        [FieldOffset(sizeof(uint))]
        public uint Int1Value;
    }

    public static class StMath
    {
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
        public static float MoveTorward(float o, float d, float s)
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
        public static Vector3 MoveTorward(Vector3 o, Vector3 d, float s)
        {
            return new Vector3
            (
                MoveTorward(o.x, d.x, s),
                MoveTorward(o.y, d.y, s),
                MoveTorward(o.z, d.z, s)
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
        public static Vector3 MoveTorward(Vector3 o, Vector3 d, Vector3 s)
        {
            return new Vector3
            (
                MoveTorward(o.x, d.x, s.x),
                MoveTorward(o.y, d.y, s.y),
                MoveTorward(o.z, d.z, s.z)
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