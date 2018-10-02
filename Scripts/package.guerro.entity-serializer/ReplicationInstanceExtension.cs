using Unity.Entities;

namespace Scripts.Utilities
{
    public static class ReplicationInstanceExtension
    {
        public static int GetId(this IReplicationInstance instance)
        {
            var world = World.Active;
            var mgr = world.GetExistingManager<ReplicationInstanceManager>();
            
            return mgr.GetId(instance);
        }
    }
}