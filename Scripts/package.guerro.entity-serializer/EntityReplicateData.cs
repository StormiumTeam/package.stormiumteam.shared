using System;
using Unity.Entities;

namespace Scripts.Utilities
{
    public enum ReplicationUpdate
    {
        Default = 0,
        Never = 1,
        Once = 2,
        Everytime = 3
    }

    public enum ReplicationComponentCreation
    {
        Never = 1,
        AutomaticallyCreate = 2,
    }
    
    public struct ReplicatedEntity
    {
        public ReplicationUpdate DefaultUpdateMode;
        public ReplicationComponentCreation ComponentCreation;
        
        /// <summary>
        /// The replication instance id
        /// </summary>
        public int ReplicationInstanceId;
        /// <summary>
        /// The original entity of the replication link
        /// </summary>
        public Entity Original;

        /// <summary>
        /// Get the instance that can manage this entity
        /// </summary>
        /// <returns></returns>
        public IReplicationInstance GetReplicationInstance()
        {
            return World.Active.GetExistingManager<ReplicationInstanceManager>().GetInstance(ReplicationInstanceId);
        }

        /// <summary>
        /// Is the entity valid?
        /// </summary>
        /// <returns></returns>
        public bool IsValid() => Original != Entity.Null && ReplicationInstanceId != 0;
        /// <summary>
        /// Is this the original entity?
        /// </summary>
        /// <returns></returns>
        public bool IsOriginal(Entity compared) => Original == compared;

        public ReplicatedEntity(Entity original, IReplicationInstance instance)
        {
            Original = original;
            // We don't want the id to be directly put inside the entity (in case of getting the id from a networked server, it wouldn't be correct)
            ReplicationInstanceId = instance.GetId();

            DefaultUpdateMode = ReplicationUpdate.Everytime;
            ComponentCreation = ReplicationComponentCreation.AutomaticallyCreate;
        }
    }

    public struct ReplicatedEntityComponentBuffer : IBufferElementData
    {
        public ReplicationUpdate UpdateMode;
        public int TypeIndex;

        public Type GetManagedType() => TypeManager.GetType(TypeIndex);
    }
}