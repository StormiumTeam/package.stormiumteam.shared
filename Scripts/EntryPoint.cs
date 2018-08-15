using package.stormiumteam.shared;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    [DefaultExecutionOrder(1)]
    public class EntryPoint
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            var worldSettings = new World("Settings");
            
            ECSWorldLoop.FlagAsLoopable(World.Active);
            
            new GameObject("Native", typeof(NativeUnitySystem));
        }
    }
}