using Unity.Entities;
using UnityEngine;

namespace package.guerro.shared
{
    public class EntryPoint
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            var worldSettings = new World("Settings");
        }
    }
}