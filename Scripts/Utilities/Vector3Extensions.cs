using UnityEngine;

namespace package.guerro.shared
{
    public static class Vector3Extensions
    {
        public static Vector3 ToGrid(this Vector3 vec, int axis)
        {
            if (axis == 1)
                return new Vector3(vec.x, 0, vec.z);
            return vec;
        }
    }
}