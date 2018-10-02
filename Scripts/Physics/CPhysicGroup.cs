using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    public class CPhysicGroupManaged
    {
        private int m_Version;
        private int m_PreviousVersion;
        
        public List<GameObject> GameObjects;

        public CPhysicGroupManaged()
        {
            GameObjects = new List<GameObject>(64);
        }

        public GameObject InsertGameObject
        {
            set
            {
                GameObjects.Add(value);

                m_Version++;
            }
        }
        
        public GameObject PopGameObject
        {
            set
            {
                GameObjects.Remove(value);
            }
        }

        public bool IncorrectVersion()
        {
            return m_Version != m_PreviousVersion;
        }

        public void PushNewVersion()
        {
            m_PreviousVersion = m_Version;
        }
    }
    
    public struct CPhysicGroup
    {
        public CPhysicGroupManaged Managed;
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

    public struct PhysicGroupTargetData : IBufferElementData
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

        public CPhysicGroup GetGroup()
        {
            return World.Active.GetExistingManager<EntityManager>()
                        .GetSharedComponentData<PhysicGroupData>(Target)
                        .Get();
        }
    }
}