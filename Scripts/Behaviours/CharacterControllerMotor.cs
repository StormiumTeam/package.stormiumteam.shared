using System;
using System.Collections.Generic;
using Scripts.Physics;
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

        public bool IsGroundForcedThisFrame
        {
            get
            {
                CleanLastFrameEvents();
                return m_IsGroundForcedThisFrame;
            }
            set => m_IsGroundForcedThisFrame = value;
        }

        // Add the component 'StackData' to the entity
        // Because this is a very small code, we don't use a system
        private void Awake()
        {
            // Get our entity by using the referencable gameobject.
            var gameObjectEntity = ReferencableGameObject.GetComponent<GameObjectEntity>(gameObject);
            if (gameObjectEntity == null)
                gameObjectEntity = gameObject.AddComponent<GameObjectEntity>();

            m_CharacterController = GetComponent<CharacterController>();

            // Create the variables
            var e = gameObjectEntity.Entity;
            var m = gameObjectEntity.EntityManager;

            // Add the component if it doesn't exist
            if (!m.HasComponent<StackData>(e))
                m.AddComponentData(e, new StackData());
            
            CPhysicSettings.Active.SetDefaultGroup(e);
        }

        private void OnDestroy()
        {
            m_AllColliderHitsInFrame.Clear();
            m_AllMoveEventsInFrame.Clear();

            m_AllColliderHitsInFrame = null;
            m_AllMoveEventsInFrame = null;
            m_CharacterController = null;
        }

        /*public CPhysicGroup GetGroup()
        {
            var gameObjectEntity = ReferencableGameObject.GetComponent<GameObjectEntity>(gameObject);
            
            var e = gameObjectEntity.Entity;
            var m = gameObjectEntity.EntityManager;

            return m.GetComponentData<PhysicGroupTargetData>(e).Target;
        }*/

        /// <summary>
        /// Check if we are on a slope.
        /// This is performance heavy, so you should do your own method.
        /// </summary>
        /// <returns></returns>
        public bool IsOnSlope()
        {
            return false;
            // We are in air, and floating slopes don't exist.
            if (!IsGrounded())
                return false;

            /*RaycastHit hit;
            
            var worldCenter = transform.position + m_CharacterController.center;
            
            var lowPoint  = worldCenter - new Vector3(0, m_CharacterController.height * 0.5f, 0);
            var highPoint = lowPoint + new Vector3(0, m_CharacterController.height, 0);

            CPhysicSettings.Active.SetCollision(GetGroup(), false);
            
            var ray = new Ray(worldCenter, Vector3.down);
            var radius = m_CharacterController.radius - m_CharacterController.skinWidth;
            if (Physics.SphereCast(ray, radius, m_CharacterController.height * 2))
            {
                
            }
            
            CPhysicSettings.Active.SetCollision(GetGroup(), true);
            */
            return true;
        }

        public bool IsGrounded()
        {
            if (m_CharacterController.isGrounded)
                return true;
            if (m_CharacterController.collisionFlags == CollisionFlags.CollidedBelow)
                return true;
            if (IsGroundForcedThisFrame)
                return true;
            
            /*var worldCenter = transform.position + m_CharacterController.center;
            
            var lowPoint = worldCenter - new Vector3(0, m_CharacterController.height * 0.5f, 0);
            var highPoint = lowPoint + new Vector3(0, m_CharacterController.height, 0);

            if (Physics.CapsuleCast(lowPoint, highPoint, m_CharacterController.radius, 
                Vector3.down, m_CharacterController.skinWidth + 5f + (m_CharacterController.height * 0.5f)))
                return true;*/
            
            return false;
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

                m_IsGroundForcedThisFrame = false;
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
            /// <summary>
            /// The friction force of the character (manual implementation)
            /// </summary>
            public float   FrictionForce;
            /// <summary>
            /// The velocity of the character (manual implementation)
            /// </summary>
            public Vector3 Velocity;
            /// <summary>
            /// The mass of the character, the higher it is, the more it should push objects far away (manual implementation)
            /// </summary>
            public float Mass;
        }
    }

    public partial class CharacterControllerMotor
    {
        private  CharacterController         m_CharacterController;
        internal List<MoveEvent>             m_AllMoveEventsInFrame   = new List<MoveEvent>();
        internal List<ControllerColliderHit> m_AllColliderHitsInFrame = new List<ControllerColliderHit>();
        internal bool                        m_IsGroundForcedThisFrame = false;
        private  int                         m_LastCleanFrame = 0;
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