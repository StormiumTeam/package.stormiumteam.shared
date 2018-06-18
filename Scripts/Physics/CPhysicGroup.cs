using Unity.Entities;

namespace Scripts.Physics
{
    public struct CPhysicGroup
    {
        public string Name;
        public Entity Id;
    }

    public struct PhysicGroupData : ISharedComponentData
    {
        internal CPhysicGroup Group;
        
        public CPhysicGroup Get()
        {
            return Group;
        }
    }

    public struct PhysicGroupTargetData : IComponentData
    {
        public Entity Target;

        public PhysicGroupTargetData(CPhysicGroup group)
        {
            Target = group.Id;
        }

        public PhysicGroupTargetData(PhysicGroupData groupData)
        {
            Target = groupData.Group.Id;
        }
    }
}