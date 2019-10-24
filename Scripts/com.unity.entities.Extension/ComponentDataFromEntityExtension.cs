using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace package.stormiumteam.shared.ecs
{
	public static class ComponentDataFromEntityExtension
	{
		public static T TryGet<T>(this ComponentDataFromEntity<T> cdfe, Entity entity, out bool hasComponent, T defaultValue)
			where T : struct, IComponentData
		{
			if (cdfe.Exists(entity))
			{
				hasComponent = true;
				return cdfe[entity];
			}

			hasComponent = false;
			return defaultValue;
		}
		
		public static unsafe void TrySet<T>(this ComponentDataFromEntity<T> cdfe, Entity entity, T value, bool compareChange)
			where T : struct, IComponentData
		{
			if (cdfe.Exists(entity))
			{
				if (compareChange)
				{
					var oldValue = cdfe[entity];
					if (UnsafeUtility.MemCmp(UnsafeUtility.AddressOf(ref oldValue), UnsafeUtility.AddressOf(ref value), UnsafeUtility.SizeOf<T>()) != 0)
					{
						cdfe[entity] = value;
						return;
					}
				}
				cdfe[entity] = value;
			}
		}
	}
}