using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    public class CPhysicTracer : ComponentSystem
    {
        public static CPhysicTracer Active => Unity.Entities.World.Active.GetOrCreateManager<CPhysicTracer>();
        
        protected override void OnUpdate()
        {
            
        }

        public TracerColliderResult<TCollider> Get<TCollider>()
            where TCollider : Collider
        {
            if (TracerColliderResult<TCollider>.m_Collider == null)
            {
                TracerColliderResult<TCollider>.m_Collider = new GameObject($"#CPhysicTracer({typeof(TCollider).Name})")
                    .AddComponent<TCollider>();
            }

            var collider = TracerColliderResult<TCollider>.m_Collider;
            collider.gameObject.layer = 31;
            collider.transform.parent = null;
            collider.transform.position = Vector3.zero;
            collider.transform.localScale = Vector3.one;
            collider.transform.rotation = Quaternion.identity;

            ResetCollider(collider);
            
            return new TracerColliderResult<TCollider>(collider);
        }

        public void Pool<TCollider>(TCollider collider)
            where TCollider : Collider
        {
            collider.gameObject.layer = 31;
        }
        
        public void ResetCollider<TCollider>(TCollider collider)
            where TCollider : Collider
        {
            var type = typeof(TCollider);
            var mkr = __makeref(collider);
            if (type == typeof(CapsuleCollider))
            {
                var capsuleCollider = __refvalue(mkr, CapsuleCollider);
                capsuleCollider.center = Vector3.zero;
                capsuleCollider.direction = 1;
                capsuleCollider.height = 2;
                capsuleCollider.radius = 0.5f;
            }
        }
    }


    public struct TracerColliderResult<TCollider> : IDisposable
        where TCollider : Collider
    {
        internal static TCollider m_Collider;
        
        public TCollider Collider;

        public TracerColliderResult(TCollider collider)
        {
            Collider = collider;
        }
        
        public void Dispose()
        {
            CPhysicTracer.Active.Pool(Collider);
            Collider = null;
        }
        
        public static implicit operator TCollider(TracerColliderResult<TCollider> result)
        {
            return result.Collider;
        }
    }
}