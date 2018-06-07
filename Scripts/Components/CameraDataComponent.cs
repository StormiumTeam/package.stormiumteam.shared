using System;
using Unity.Entities;
using UnityEngine;

namespace package.guerro.shared
{
    [Serializable]
    public struct CameraData : IComponentData
    {
        [Header("Internal")]
        public int CameraId;

        [Header("Properties")]
        public Vector3 Position;
        public Quaternion Rotation;
        public float FieldOfView;
    }

    public class CameraDataComponent : BetterComponentWrapper<CameraData>
    {
        
    }
}