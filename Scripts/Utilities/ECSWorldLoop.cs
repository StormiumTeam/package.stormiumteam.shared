using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Entities;

namespace package.stormiumteam.shared
{
    public static class ECSWorldLoop
    {
        private static List<World> m_LoopableWorlds = new List<World>();

        public static ReadOnlyCollection<World> LoopableWorlds => new ReadOnlyCollection<World>(m_LoopableWorlds);
        public static long Version { get; private set; }

        public static void FlagAsLoopable(World world)
        {
            if (!m_LoopableWorlds.Contains(world)) m_LoopableWorlds.Add(world);
        }

        public static void FlagAsUnloopable(World world)
        {
            if (m_LoopableWorlds.Contains(world)) m_LoopableWorlds.Remove(world);
        }

        public static void Retake()
        {
            FlagAsLoopable(World.Active);

            var copy = m_LoopableWorlds.ToArray();
            foreach (var world in copy)
            {
                if (!World.AllWorlds.Contains(world))
                    FlagAsUnloopable(world);
            }
            
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(copy);

            Version++;
        }
    }
}