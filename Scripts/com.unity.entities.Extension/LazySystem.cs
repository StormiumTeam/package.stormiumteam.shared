#define LAZY_USE_WORLD_ACTIVE

using System.Runtime.CompilerServices;
using Unity.Entities;

namespace package.stormiumteam.shared.ecs
{
	public struct LazySystem<T> where T : ComponentSystemBase
	{
		private bool m_Initialized;

		public T Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(LazySystem<T> lazy)
		{
#if !LAZY_USE_WORLD_ACTIVE
			if (!lazy.m_Initialized)
				throw new InvalidOperationException(string.Format("Lazy<{0}> not initialized.", typeof(T)));
#else
			if (!lazy.m_Initialized)
				return lazy.Get(World.Active);
#endif

			return lazy.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get(World world, bool getOrCreate = true)
		{
			if (m_Initialized)
				return Value;

			m_Initialized = true;
			return Value = getOrCreate ? world.GetOrCreateSystem<T>() : world.GetExistingSystem<T>();
		}
	}

	public static class ComponentSystemBaseExtensions
	{
		public static T L<T>(this ComponentSystemBase system, ref LazySystem<T> lazy, bool getOrCreate = true)
			where T : ComponentSystemBase
		{
			return lazy.Get(system.World, getOrCreate);
		}
	}
}