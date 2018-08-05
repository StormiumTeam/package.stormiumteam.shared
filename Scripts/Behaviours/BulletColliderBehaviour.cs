using System;
using System.Collections.Generic;
using package.stormiumteam.shared;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    [RequireComponent(typeof(ReferencableGameObject))]
    public partial class BulletColliderBehaviour : MonoBehaviour
    {
        public enum RoleType
        {
            Victim = 1,
            Offender = 2,
            Both = 3
        }
        
        public RoleType Role = RoleType.Both;
        
        public void InvokeCollision(BulletColliderBehaviour victim, Vector3 worldPosition, Vector3 worldNormal)
        {
            InvokeCollision(this, victim, worldPosition, worldNormal);
        }

        public static void InvokeCollision(BulletColliderBehaviour offender,      BulletColliderBehaviour victim,
                                           Vector3                 worldPosition, Vector3                 worldNormal)
        {
            if (offender.Role == RoleType.Victim)
                throw new Exception($"The role of the offender {offender.gameObject.name} is '{offender.Role.ToString()}'");
            if (victim.Role == RoleType.Offender)
                throw new Exception($"The role of the victim {victim.gameObject.name} is '{victim.Role.ToString()}'");
            
            // Create match
            var match = new IdMatch(offender.gameObject.GetInstanceID(), victim.gameObject.GetInstanceID());
            
            Debug.Log($"New bullet collision! oid: {match.LeftId} vid: {match.RightId}");
        }
    }

    public partial class BulletColliderBehaviour
    {
        public struct IdMatch
        {
            private sealed class LeftIdRightIdEqualityComparer : IEqualityComparer<IdMatch>
            {
                public bool Equals(IdMatch x, IdMatch y)
                {
                    return x.LeftId == y.LeftId && x.RightId == y.RightId;
                }

                public int GetHashCode(IdMatch obj)
                {
                    unchecked
                    {
                        return (obj.LeftId * 397) ^ obj.RightId;
                    }
                }
            }

            public static IEqualityComparer<IdMatch> LeftIdRightIdComparer { get; } =
                new LeftIdRightIdEqualityComparer();

            public int LeftId;
            public int RightId;

            public IdMatch(int leftId, int rightId)
            {
                LeftId  = leftId;
                RightId = rightId;
            }
        }

        public struct CollisionResultData : IComponentData
        {
            public IdMatch Uid;
            public int     VictimId;
            public int     OffenderId;
            public Vector3 WorldPosition;
            public Vector3 WorldNormal;

            public BulletColliderBehaviour VictimBullet =>
                ReferencableGameObject.GetComponent<BulletColliderBehaviour>(VictimId);
        }
    }
}