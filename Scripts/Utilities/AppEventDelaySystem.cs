using System;
using package.stormiumteam.shared;
using Unity.Collections;
using Unity.Entities;

namespace package.stormiumteam.shared
{
    public struct DelayEntityTag : IComponentData
    {
    }

    public struct DelayEntitySystem<TSystem> : IComponentData
    {
    }

    public struct DelayEntityTargetSystem<TTargetSystem> : IComponentData
    {

    }

    public interface IDelayComponentArguments : ISharedComponentData
    {
    }

    public interface IAppEventDelayGroup<TDelaySystem, TArguments, TTarget>
        where TTarget : ComponentSystem
        where TArguments : struct, IDelayComponentArguments
    {
    }

    public static class DelayGroupExtensions
    {
        public static void CreateDelayedEntity<TDelaySystem, TArguments, TTarget>
            (this IAppEventDelayGroup<TDelaySystem, TArguments, TTarget> group, TArguments arguments)
            where TTarget : ComponentSystem
            where TArguments : struct, IDelayComponentArguments
        {
            var em = World.Active.GetOrCreateManager<EntityManager>();
            var entity = em.CreateEntity
            (
                typeof(DelayEntityTag),
                typeof(DelayEntitySystem<TDelaySystem>),
                typeof(DelayEntityTargetSystem<TTarget>)
            );
            em.SetOrAddSharedComponentData(entity, arguments);
        }

        public static ComponentType[] GetComponentGroup<TDelaySystem, TArguments, TTarget>
            (this IAppEventDelayGroup<TDelaySystem, TArguments, TTarget> group)
            where TTarget : ComponentSystem
            where TArguments : struct, IDelayComponentArguments
        {
            return new ComponentType[]
            {
                typeof(DelayEntityTag),
                typeof(DelayEntitySystem<TDelaySystem>),
                typeof(DelayEntityTargetSystem<TTarget>),
                typeof(TArguments)
            };
        }

        public static void ClearAll<TDelaySystem, TArguments, TTarget>
            (this IAppEventDelayGroup<TDelaySystem, TArguments, TTarget> group, ComponentGroup componentGroup, CmdBuffer buffer = default (CmdBuffer))
            where TTarget : ComponentSystem
            where TArguments : struct, IDelayComponentArguments
        {
            var commandBuffer = buffer.Buffer ?? new EntityCommandBuffer(Allocator.Temp);
            
            var entities = componentGroup.GetEntityArray();
            for (int i = 0; i != entities.Length; i++)
            {
                commandBuffer.DestroyEntity(entities[i]);
            }

            if (buffer.Buffer == null)
            {
                commandBuffer.Playback(World.Active.GetExistingManager<EntityManager>());
                commandBuffer.Dispose();
            }
        }
    }
}