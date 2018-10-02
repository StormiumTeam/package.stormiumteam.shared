using Unity.Entities;

namespace Scripts.Utilities
{
    public interface IReplicationInstance
    {
        void OnRegisterInstance();
        void OnReplicateEntity(World world, Entity entity);
    }
}