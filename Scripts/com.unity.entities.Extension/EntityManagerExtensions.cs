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
				em.SetComponentData(entity, data);
			}
			else
			{
				em.AddComponentData(entity, data);
			}
		}

		public static bool TryGetBuffer<T>(this EntityManager em, Entity entity, out DynamicBuffer<T> data)
			where T : struct, IBufferElementData
		{
			if (em.HasComponent<T>(entity))
			{
				data = em.GetBuffer<T>(entity);
				return true;
			}

			data = default;
			return false;
		}

		public static DynamicBuffer<T> GetOrAddBuffer<T>(this EntityManager em, Entity entity) 
			where T : struct, IBufferElementData
		{
			if (TryGetBuffer(em, entity, out DynamicBuffer<T> buffer))
				return buffer;

			buffer = em.AddBuffer<T>(entity);
			return buffer;
		}

		public static bool TryGetComponentData<T>(this EntityManager em, Entity entity, out T data, T def = default)
			where T : struct, IComponentData
		{
			if (em.HasComponent<T>(entity))
			{
				data = em.GetComponentData<T>(entity);
				return true;
			}

			data = def;
			return false;
		}
		
		public static bool TryGetComponent<T>(this EntityManager em, Entity entity, out T data, T def = default)
			where T : class, IComponentData
		{
			if (em.HasComponent<T>(entity))
			{
				data = em.GetComponentData<T>(entity);
				return true;
			}

			data = def;
			return false;
		}
	}
}