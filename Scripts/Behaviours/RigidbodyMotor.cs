using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    /// <summary>
    /// Provide an extension to the rigidbody. 3D only
    /// </summary>
    public partial class RigidbodyMotor : MonoBehaviour
    {
        /*public event Action<ControllerColliderHit> OnHit;

        public Rigidbody Rigidbody
        {
            get { return m_Rigidbody; }
        }

        public IReadOnlyList<MoveEvent> AllMoveEventsInFrame
        {
            get
            {
                CleanLastFrameEvents();
                return m_AllMoveEventsInFrame;
            }
        }

        public IReadOnlyList<ControllerColliderHit> AllColliderHitsInFrame
        {
            get
            {
                CleanLastFrameEvents();
                return m_AllColliderHitsInFrame;
            }
        }

        // Add the component 'StackData' to the entity
        // Because this is a very small code, we don't use a system
        private void Awake()
        {
            // Get our entity by using the referencable gameobject.
            var gameObjectEntity = ReferencableGameObject.GetComponent<GameObjectEntity>(gameObject);
            if (gameObjectEntity == null)
                gameObjectEntity = gameObject.AddComponent<GameObjectEntity>();

            // Create the variables
            var e = gameObjectEntity.Entity;
            var m = gameObjectEntity.EntityManager;

            // Add the component if it doesn't exist
            if (!m.HasComponent<StackData>(e))
                m.AddComponentData(e, new StackData());
        }

        /// <summary>
        /// Get move events when an idle motion
        /// </summary>
        /// <returns></returns>
        public MoveEvent Zero()
        {
            return MoveBy(new Vector3());
        }

        /// <summary>
        /// Teleport the controller to a given position, with physics rules (collision, etc...).
        /// If you want to teleport the player without physics rule, just modify the transform position
        /// </summary>
        /// <param name="toPosition"></param>
        /// <returns></returns>
        public MoveEvent MoveTo(Vector3 toPosition)
        {
            return MoveBy(transform.position - toPosition);
        }

        public MoveEvent MoveBy(Vector3 motion)
        {
            CleanLastFrameEvents();

            var originPosition = m_Rigidbody.transform.position;

            var eventListenerStartIndex = m_AllColliderHitsInFrame.Count;
            m_Rigidbody.MovePosition(originPosition + motion);
            var eventListenerEndIndex = m_AllColliderHitsInFrame.Count;

            return new MoveEvent()
            {
                RigidbodyMotor = this,
                Motion                   = motion,
                Origin                   = originPosition,
                Position                 = m_Rigidbody.transform.position,
                EventsStartIndex         = eventListenerStartIndex,
                EventsLength             = eventListenerEndIndex - eventListenerStartIndex
            };
        }

        private void CleanLastFrameEvents()
        {
            if (m_LastCleanFrame < Time.frameCount)
            {
                m_LastCleanFrame = Time.frameCount;

                m_AllColliderHitsInFrame.Clear();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            m_AllColliderHitsInFrame.Add(hit);
        }*/
    }

    public partial class RigidbodyMotor
    {
    }

    public partial class RigidbodyMotor
    {
        private  Rigidbody                   m_Rigidbody;
        internal List<ControllerColliderHit> m_AllColliderHitsInFrame = new List<ControllerColliderHit>();
        private  int                         m_LastCleanFrame         = 0;
    }

    /*public class STCharacterControllerSystem : ComponentSystem
    {
        public struct Group
        {
            public ComponentArray<CGameCharacterController> Controllers;
            public ComponentDataArray<CGameCharacterController.StackData> Datas;
            public int Length;
        }

        private Group m_Group;

        protected override void OnUpdate()
        {
            for (int i = 0; i != m_Group.Length; i++)
            {
                var controller = m_Group.Controllers[i];
                var data = m_Group.Datas[i];
                
                
            }
        }
    }*/
}