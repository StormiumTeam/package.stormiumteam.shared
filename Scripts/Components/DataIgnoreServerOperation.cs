using Unity.Entities;

namespace package.stormiumteam.shared
{
    public struct DataIgnoreServerOperation<T> : IComponentData
        where T : struct, IComponentData
    {
        
    }

    public struct IgnoreServerOperation<T> : IComponentData
    {
    }
}