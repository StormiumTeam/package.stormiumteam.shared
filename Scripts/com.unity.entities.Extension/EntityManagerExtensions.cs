using Unity.Entities;

namespace package.stormiumteam.shared.ecs
{
	public static class EntityManagerExtensions
	{
		public static void SetOrAddSharedComponentData<T>(this EntityManager em, Entity entity, T data)
			where T : struct, ISharedComponentData
		{
			if (em.HasComponent<T>(entity))
				em.SetSharedComponentData(entity, data);
			else
				em.AddSharedComponentData(entity, data);
		}

		public static void SetOrAddComponentData<T>(this EntityManager em, Entity entity, T data)
			where T : struct, IComponentData
		{
			if (em.HasComponent<T>(entity))
			{
				if (TypeManager.GetTypeInfo<T>().IsZeroSized)
					return;

				em.SetComponentData(entity, data);
			}
			else
				em.AddComponentData(entity, data);
		}
	}
}