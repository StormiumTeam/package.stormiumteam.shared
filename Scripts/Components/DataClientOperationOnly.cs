using Unity.Entities;

namespace package.stormiumteam.shared
{
    public struct DataClientOperationOnly<T> : IComponentData
        where T : struct, IComponentData
    {
        
    }

    public struct ClientOperationOnly<T> : IComponentData
    {
        
    }
}