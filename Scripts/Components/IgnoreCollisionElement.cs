using Unity.Entities;

namespace package.stormiumteam.shared
{
    // TODO: See if we should change internal Capacity
    public struct IgnoreCollisionElement : IBufferElementData
    {        
        public Entity TargetEntity;
        public int TargetGameObjectRef;

        public IgnoreCollisionElement(Entity targetEntity)
        {
            TargetEntity        = targetEntity;
            TargetGameObjectRef = -1;
        }
        
        public IgnoreCollisionElement(int targetGameObjectRef)
        {
            TargetGameObjectRef = targetGameObjectRef;
            TargetEntity = Entity.Null;
        }

        public IgnoreCollisionElement(Entity targetEntity, int targetGameObjectRef)
        {
            TargetEntity = targetEntity;
            TargetGameObjectRef = targetGameObjectRef;
        }
    }
}