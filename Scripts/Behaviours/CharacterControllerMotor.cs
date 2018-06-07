using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace package.guerro.shared
{
    /// <summary>
    /// Provide an extension to the charactercontroller. 3D only
    /// </summary>
    public partial class CharacterControllerMotor : MonoBehaviour
    {
        public event Action<ControllerColliderHit> OnHit;

        public CharacterController CharacterController
        {
            get { return m_CharacterController; }
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

            var originPosition = m_CharacterController.transform.position;

            var eventListenerStartIndex = m_AllColliderHitsInFrame.Count;
            m_CharacterController.Move(motion);
            var eventListenerEndIndex = m_AllColliderHitsInFrame.Count;

            return new MoveEvent()
            {
                CharacterControllerMotor = this,
                Motion                   = motion,
                Origin                   = originPosition,
                Position                 = m_CharacterController.transform.position,
                EventsStartIndex         = eventListenerStartIndex,
                EventsLength             = eventListenerEndIndex - eventListenerStartIndex
            };
        }

        private void CleanLastFrameEvents()
        {
            if (m_LastCleanFrame < Time.frameCount)
            {
                m_LastCleanFrame = Time.frameCount;

                m_AllMoveEventsInFrame.Clear();
                m_AllColliderHitsInFrame.Clear();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            m_AllColliderHitsInFrame.Add(hit);
        }
    }

    public partial class CharacterControllerMotor
    {
        public struct MoveEvent
        {
            public CharacterControllerMotor CharacterControllerMotor;

            public Vector3 Motion;
            public Vector3 Origin;
            public Vector3 Position;

            public int EventsStartIndex;
            public int EventsLength;

            public ControllerColliderHit GetColliderHit(int index)
            {
                if (index > EventsLength)
                    throw new Exception("error todo");
                return CharacterControllerMotor.m_AllColliderHitsInFrame[EventsStartIndex + index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public struct StackData : IComponentData
        {
            public float   FrictionForce;
            public Vector3 Velocity;
        }
    }

    public partial class CharacterControllerMotor
    {
        private  CharacterController         m_CharacterController;
        internal List<MoveEvent>             m_AllMoveEventsInFrame   = new List<MoveEvent>();
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