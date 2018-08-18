using Unity.Entities;

namespace package.stormiumteam.shared
{
    public struct CmdBuffer
    {
        public bool IsCreated;
        public bool                 UseBuffering;
        public EntityCommandBuffer? Buffer;

        private EntityCommandBuffer m_Buffer;
        private EntityManager       m_EntityManager;

        public CmdBuffer(EntityManager manager)
        {
            UseBuffering    = false;
            Buffer          = null;
            m_Buffer        = default(EntityCommandBuffer);
            m_EntityManager = manager ?? World.Active.GetOrCreateManager<EntityManager>();

            IsCreated = true;
        }

        public CmdBuffer(EntityCommandBuffer? buffer, EntityManager entityManager = null)
        {
            UseBuffering = buffer != null;

            Buffer          = buffer;
            m_EntityManager = entityManager ?? World.Active.GetOrCreateManager<EntityManager>();

            m_Buffer = Buffer ?? default(EntityCommandBuffer);
            
            IsCreated = true;
        }

        public CmdBuffer(CmdBuffer original, CmdBuffer implementation, EntityManager entityManager = null)
        {
            UseBuffering    = implementation.UseBuffering;
            Buffer          = UseBuffering ? (implementation.Buffer ?? original.Buffer) : null;
            m_EntityManager = entityManager ?? World.Active.GetOrCreateManager<EntityManager>();

            if (Buffer == null)
            {
                UseBuffering = false;
                m_Buffer     = default(EntityCommandBuffer);
            }
            else m_Buffer = Buffer.Value;
            
            IsCreated = true;
        }

        public void AddComponentData<T>(Entity entity, T data)
            where T : struct, IComponentData
        {
            if (UseBuffering)
            {
                m_Buffer.AddComponent(entity, data);
                return;
            }

            if (!IsCreated) m_EntityManager = World.Active.GetOrCreateManager<EntityManager>();
            
            m_EntityManager.AddComponentData(entity, data);
        }

        public void SetOrAddComponentData<T>(Entity entity, T data)
            where T : struct, IComponentData
        {
            if (UseBuffering)
            {
                if (m_EntityManager.HasComponent<T>(entity))
                    m_Buffer.SetComponent(entity, data);
                else
                    m_Buffer.AddComponent(entity, data);
                return;
            }

            if (!IsCreated) m_EntityManager = World.Active.GetOrCreateManager<EntityManager>();

            if (m_EntityManager.HasComponent<T>(entity))
                m_EntityManager.SetComponentData(entity, data);
            else
                m_EntityManager.AddComponentData(entity, data);
        }
        
        public static CmdBuffer Resolve(BarrierSystem original)
        {
            return new CmdBuffer(original.CreateCommandBuffer());
        }
        
        public static CmdBuffer Resolve(BarrierSystem original, CmdBuffer against)
        {
            return new CmdBuffer(new CmdBuffer(original.CreateCommandBuffer()), against);
        }

        public static CmdBuffer Resolve(EntityCommandBuffer original, CmdBuffer against)
        {
            return new CmdBuffer(new CmdBuffer(original), against);
        }
    }
}