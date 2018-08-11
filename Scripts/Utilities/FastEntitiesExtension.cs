using Unity.Entities;

namespace package.stormiumteam.shared
{
    public static class FastEntitiesExtensions
    {
        public static bool HasComponent<T>(this Entity entity, World world = null)
        {
            if (world == null)
                world = World.Active;
            return world.GetExistingManager<EntityManager>().HasComponent<T>(entity);
        }

        public static T GetComponentData<T>(this Entity entity, World world = null)
            where T : struct, IComponentData
        {
            if (world == null)
                world = World.Active;
            return world.GetExistingManager<EntityManager>().GetComponentData<T>(entity);
        }

        public static void SetComponentData<T>(this Entity entity, T data, World world = null)
            where T : struct, IComponentData
        {
            if (world == null)
                world = World.Active;
            world.GetExistingManager<EntityManager>().SetComponentData(entity, data);
        }

        public static void SetOrAddComponentData<T>(this Entity entity, T data, World world = null)
            where T : struct, IComponentData
        {
            if (world == null)
                world = World.Active;
            var entityManager = world.GetExistingManager<EntityManager>();
            if (entityManager.HasComponent<T>(entity))
                entityManager.SetComponentData(entity, data);
            else
                entityManager.AddComponentData(entity, data);
        }

        public static void SetOrAddSharedComponentData<T>(this Entity entity, T data, World world = null)
            where T : struct, ISharedComponentData
        {
            if (world == null)
                world = World.Active;
            var entityManager = world.GetExistingManager<EntityManager>();
            if (entityManager.HasComponent<T>(entity))
                entityManager.SetSharedComponentData(entity, data);
            else
                entityManager.AddSharedComponentData(entity, data);
        }

        public static void SetOrAddSharedComponentData<T>(this EntityManager em, Entity entity, T data)
            where T : struct , ISharedComponentData
        {
            if (em.HasComponent<T>(entity)) em.SetSharedComponentData(entity, data);
            else em.AddSharedComponentData(entity, data);
        }
    }
}