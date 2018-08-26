using Unity.Entities;
using Unity.Mathematics;

namespace package.stormiumteam.shared
{
    public struct CameraTargetData : IComponentData
    {
        /// <summary>
        /// The camera target
        /// </summary>
        public Entity CameraId;
        /// <summary>
        /// The priority of our work
        /// </summary>
        public int Priority;

        public CameraTargetData(Entity cameraId, int priority)
        {
            CameraId = cameraId;
            Priority = priority;
        }
    }
    
    public struct CameraTargetPosition : IComponentData
    {
        /// <summary>
        /// The target position
        /// </summary>
        public float3 Value;

        public CameraTargetPosition(float3 position)
        {
            Value = position;
        }
    }
    
    public struct CameraTargetRotation : IComponentData
    {
        /// <summary>
        /// The target position
        /// </summary>
        public Quaternion Value;

        public CameraTargetRotation(Quaternion rotation)
        {
            Value = rotation;
        }
    }
}