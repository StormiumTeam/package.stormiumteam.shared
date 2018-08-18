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
        public float2 Value;

        public CameraTargetPosition(float2 position)
        {
            Value = position;
        }
    }
}