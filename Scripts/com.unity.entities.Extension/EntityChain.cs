using Unity.Entities;

namespace package.stormiumteam.shared.ecs
{
	public static class EntityChainExtensions
	{
		public static EntityChain Chain(this EntityManager em, Entity entity)
		{
			return new EntityChain
			{
				EntityManager = em,
				Entity        = entity
			};
		}

		public static CommandBufferChain Chain(this EntityCommandBuffer ecb, Entity entity)
		{
			return new CommandBufferChain
			{
				Ecb    = ecb,
				Entity = entity
			};
		}
	}

	public struct EntityChain
	{
		public EntityManager EntityManager;
		public Entity Entity;
	}

	public struct CommandBufferChain
	{
		public struct BufferChain<T>
			where T : struct, IBufferElementData
		{
			public CommandBufferChain Chain;
			public DynamicBuffer<T>   Buffer;

			public BufferChain<T> ChainAdd(T elem)
			{
				Buffer.Add(elem);
				return this;
			}

			public CommandBufferChain Add(T elem)
			{
				Buffer.Add(elem);
				return Chain;
			}

			public CommandBufferChain Return() => Chain;
		}

		public EntityCommandBuffer Ecb;
		public Entity              Entity;

		public CommandBufferChain AddComponent<T>()
			where T : struct, IComponentData
		{
			Ecb.AddComponent<T>(Entity);
			return this;
		}
		
		public CommandBufferChain RemoveComponent<T>()
			where T : struct, IComponentData
		{
			Ecb.RemoveComponent<T>(Entity);
			return this;
		}
		
		public CommandBufferChain SetComponent<T>(T data)
			where T : struct, IComponentData
		{
			Ecb.SetComponent(Entity, data);
			return this;
		}

		public BufferChain<T> SetBuffer<T>()
			where T : struct, IBufferElementData
		{
			return new BufferChain<T> {Chain = this, Buffer = Ecb.SetBuffer<T>(Entity)};
		}
	}
}