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

    public interface IDelayComponentArguments : IComponentData
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
            var em = World.Active.EntityManager;
            var entity = em.CreateEntity
            (
                typeof(DelayEntityTag),
                typeof(DelayEntitySystem<TDelaySystem>),
                typeof(DelayEntityTargetSystem<TTarget>)
            );
            em.SetComponentData(entity, arguments);
        }

        public static ComponentType[] GetEntityQuery<TDelaySystem, TArguments, TTarget>
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

        /*public static void ClearAll<TDelaySystem, TArguments, TTarget>
            (this IAppEventDelayGroup<TDelaySystem, TArguments, TTarget> group, EntityQuery EntityQuery, CmdBuffer buffer = default (CmdBuffer))
            where TTarget : ComponentSystem
            where TArguments : struct, IDelayComponentArguments
        {
            var commandBuffer = buffer.Buffer ?? new EntityCommandBuffer(Allocator.Temp);
            
            var entities = EntityQuery.GetEntityArray();
            for (int i = 0; i != entities.Length; i++)
            {
                commandBuffer.DestroyEntity(entities[i]);
            }

            if (buffer.Buffer == null)
            {
                commandBuffer.Playback(World.Active.EntityManager);
                commandBuffer.Dispose();
            }
        }*/
    }
}