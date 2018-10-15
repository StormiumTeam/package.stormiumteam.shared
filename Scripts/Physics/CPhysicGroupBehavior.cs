using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    public enum EBehaviourCreateGroup
    {
        ScenePreset,
        Name
    }
    
    public class CPhysicGroupBehavior : MonoBehaviour
    {
        [SerializeField]
        private EBehaviourCreateGroup m_BehaviourCreateGroup;
        [SerializeField]
        private string[] m_GroupName;
        [SerializeField]
        private CPhysicGroupScenePreset m_GroupPreset;

        public string[] GroupName
        {
            get
            {
                if (m_BehaviourCreateGroup == EBehaviourCreateGroup.ScenePreset)
                {
                    return m_GroupPreset.GroupName;
                }
                
                return m_GroupName;
            }
            set
            {
                m_BehaviourCreateGroup = EBehaviourCreateGroup.Name;
                
                m_GroupName = value;
                RefreshInternal();
            }
        }

        private void Awake()
        {
            if (!GetComponent<GameObjectEntity>())
            {
                gameObject.AddComponent<GameObjectEntity>();
            }

            
            RefreshInternal();

            var groups = CPhysicSettings.Active.GetGroups(GetComponent<GameObjectEntity>().Entity);
            Debug.Assert(groups.exist, "groups.exist");
            for (int i = 0; i != groups.buffer.Length; i++)
            {
                var b = groups.buffer[i];
                var group = b.GetGroup();
                
                Debug.Log(group.Name);
            }
        }

        private void OnValidate()
        {
            RefreshInternal();
        }

        private void RefreshInternal()
        {
            if (!Application.isPlaying) return;
            
            var physicSettings = CPhysicSettings.Active;

            var groupList = new List<CPhysicGroup>();
            for (int i = 0; i != GroupName.Length; i++)
            {
                var group = physicSettings.CreateOrGetGroup(GroupName[i]);
                Debug.Log(group.Id + ", " + group.Name);
                groupList.Add(group);
            }

            physicSettings.SetGroups(gameObject, groupList.ToArray());
        }
    }
}