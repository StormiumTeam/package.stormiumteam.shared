using System.Collections.Generic;
using package.guerro.shared;
using Unity.Entities;
using UnityEngine;

namespace package.guerro.shared
{
    [RequireComponent(typeof(ReferencableGameObject))]
    public partial class BulletColliderBehaviour : MonoBehaviour
    {
        public void InvokeCollision(BulletColliderBehaviour victim, Vector3 worldPosition, Vector3 worldNormal)
        {
            InvokeCollision(this, victim, worldPosition, worldNormal);
        }

        public static void InvokeCollision(BulletColliderBehaviour offender,      BulletColliderBehaviour victim,
                                           Vector3                 worldPosition, Vector3                 worldNormal)
        {
            // Create match
            var match = new IdMatch(offender.gameObject.GetInstanceID(), victim.GetInstanceID());
            int uid;
            if (!s_Matches.TryGetValue(match, out uid))
            {
                s_Matches[match] = uid = s_UniqueMatches;
                s_UniqueMatches++;
            }
            
            Debug.Log($"New bullet collision! oid: {match.LeftId} vid: {match.RightId}, uid: {uid}");
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
            public int     Uid;
            public int     VictimId;
            public int     OffenderId;
            public Vector3 WorldPosition;
            public Vector3 WorldNormal;

            public BulletColliderBehaviour VictimBullet =>
                ReferencableGameObject.GetComponent<BulletColliderBehaviour>(VictimId);
        }
    }

    public partial class BulletColliderBehaviour
    {
        private static Dictionary<IdMatch, int> s_Matches =
            new Dictionary<IdMatch, int>(IdMatch.LeftIdRightIdComparer);

        private static int s_UniqueMatches = 0;

        /// <summary>
        /// Reset the matches only when needed (example -> when ending a map)
        /// </summary>
        /// <param name="resetDictionary"></param>
        public static void ResetMatches(bool resetDictionary = true)
        {
            if (resetDictionary) s_Matches.Clear();
            s_UniqueMatches = 0;
        }
    }
}