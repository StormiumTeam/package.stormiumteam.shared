using UnityEngine;

namespace package.guerro.shared
{
    public static class Vector3Extensions
    {
        public static Vector3 ToGrid(this Vector3 vec, int axis)
        {
            switch (axis)
            {
                case 0:
                    return new Vector3(0, vec.y, vec.z);
                case 1:
                    return new Vector3(vec.x, 0, vec.z);
                case 2:
                    return new Vector3(vec.x, vec.y, 0);
            }

            return vec;
        }
    }
}