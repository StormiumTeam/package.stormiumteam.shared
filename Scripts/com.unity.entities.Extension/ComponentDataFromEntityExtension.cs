using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace package.stormiumteam.shared.ecs
{
	public static class ComponentDataFromEntityExtension
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryGet<T>(this ComponentDataFromEntity<T> cdfe, Entity entity, out bool hasComponent, T defaultValue)
			where T : struct, IComponentData
		{
			if (cdfe.HasComponent(entity))
			{
				hasComponent = true;
				return cdfe[entity];
			}

			hasComponent = false;
			return defaultValue;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGet<T>(this ComponentDataFromEntity<T> cdfe, Entity entity, out T component)
			where T : struct, IComponentData
		{
			if (cdfe.HasComponent(entity))
			{
				component = cdfe[entity];
				return true;
			}

			component = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetChain<T>(this ComponentDataFromEntity<T> cdfe, Span<Entity> entities, out T component)
			where T : struct, IComponentData
		{
			var entLength = entities.Length;
			for (var i = 0; i != entLength; i++)
			{
				if (TryGet(cdfe, entities[i], out component))
					return true;
			}

			component = default;
			return false;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe bool TryGetChain<T>(this ComponentDataFromEntity<T> cdfe, Entity* entities, int length, out T component)
			where T : struct, IComponentData
		{
			for (var i = 0; i != length; i++)
			{
				if (TryGet(cdfe, entities[i], out component))
					return true;
			}

			component = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void TrySet<T>(this ComponentDataFromEntity<T> cdfe, Entity entity, T value, bool compareChange)
			where T : struct, IComponentData
		{
			if (cdfe.HasComponent(entity))
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ComponentUpdater<T> GetUpdater<T>(this ComponentDataFromEntity<T> cdfe, Entity entity)
			where T : struct, IComponentData
		{
			ComponentUpdater<T> updater;
			updater.cdfe     = cdfe;
			updater.entity   = entity;
			updater.possess  = cdfe.HasComponent(entity);
			updater.original = updater.possess ? cdfe[entity] : default;

			return updater;
		}
	}

	public struct ComponentUpdater<T>
		where T : struct, IComponentData
	{
		public ComponentDataFromEntity<T> cdfe;
		public Entity                     entity;
		public bool                       possess;
		public T                          original;

		public ComponentUpdater<T> Out(out T val, T defaultVal = default)
		{
			val = possess ? original : defaultVal;
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Update(T value)
		{
			if (!possess)
				return;

			cdfe[entity] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void CompareAndUpdate(T value)
		{
			if (!possess)
				return;

			if (UnsafeUtility.MemCmp(UnsafeUtility.AddressOf(ref original), UnsafeUtility.AddressOf(ref value), UnsafeUtility.SizeOf<T>()) != 0) cdfe[entity] = value;
		}
	}
}