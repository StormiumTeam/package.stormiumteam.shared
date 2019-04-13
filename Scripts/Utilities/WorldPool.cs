using System;
using System.Collections.Generic;
using Unity.Entities;

namespace package.stormiumteam.shared.utils
{
    public class WorldPoolItem : IDisposable
    {
        public World World { get; }
        public EntityManager EntityManager { get; }

        internal WorldPoolItem(World world)
        {
            World = world;
            EntityManager = World.EntityManager;
        }
        
        public void Dispose()
        {
            WorldPool.ReturnToPool(this);
        }
    }

    public static class WorldPool
    {
        private static Queue<WorldPoolItem> s_Items = new Queue<WorldPoolItem>();

        internal static void ReturnToPool(WorldPoolItem worldPoolItem)
        {
            s_Items.Enqueue(worldPoolItem);
        }

        internal static void AddRange(int count)
        {
            for (int i = 0; i != count; i++)
            {
                s_Items.Enqueue(new WorldPoolItem(CreateWorld($"PooledWorld({i})")));
            }
        }

        internal static World CreateWorld(string name)
        {
            var world = new World(name);
            
            return world;
        }
        
        public static WorldPoolItem Get()
        {
            return s_Items.Count < 0
                ? null
                : s_Items.Dequeue();
        }
    }
}