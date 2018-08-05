using System;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    [Serializable]
    public struct CameraTargetData : IComponentData
    {
        public Vector3 Position;
        public Vector3 Rotation;
        
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;

        public float FieldOfView;

        public int CameraId;
    }

    public class CameraTargetComponent : BetterComponentWrapper<CameraTargetData>
    {
        
    }
}