using System.Collections.Generic;
using Unity.Entities;

namespace Scripts.Utilities
{
    public class ReplicationInstanceManager : ComponentSystem
    {
        private List<IReplicationInstance> m_AllInstances;

        protected override void OnCreate()
        {
            m_AllInstances = new List<IReplicationInstance>();
        }

        protected override void OnDestroy()
        {
            m_AllInstances.Clear();
            m_AllInstances = null;
        }

        protected override void OnUpdate()
        {
            
        }

        public IReplicationInstance GetInstance(int id)
        {
            return m_AllInstances[id];
        }

        public int GetId(IReplicationInstance instance)
        {
            return m_AllInstances.IndexOf(instance);
        }
    }
}