using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace package.stormiumteam.shared
{
    /// <summary>
    /// Provide an extension to the charactercontroller. 3D only
    /// </summary>
    public partial class CharacterControllerMotor : MonoBehaviour
    {
        public event Action<ControllerColliderHit> OnHit;

        public CharacterController CharacterController
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return m_CharacterController; }
        }

        public IReadOnlyList<MoveEvent> AllMoveEventsInFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get
            {
                CleanLastFrameEvents();
                return m_AllMoveEventsInFrame;
            }
        }

        public IReadOnlyList<ControllerColliderHit> AllColliderHitsInFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get
            {
                CleanLastFrameEvents();
                return m_AllColliderHitsInFrame;
            }
        }

        public bool IsGroundForcedThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get
            {
                CleanLastFrameEvents();
                return m_IsGroundForcedThisFrame;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { m_IsGroundForcedThisFrame = value; }
        }

        public Vector3 AngleDir
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return m_AngleDir; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { m_AngleDir = value; }
        }

        public bool IsStableOnGround
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return m_IsStableOnGround; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set { m_IsStableOnGround = value; }
        }

        public bool IsSliding { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; [MethodImpl(MethodImplOptions.AggressiveInlining)] set; }
        public Vector3 Momentum { get; set; }

        // Add the component 'StackData' to the entity
        // Because this is a very small code, we don't use a system
        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_CachedTransform = transform;

            var gameObjectEntity = GetComponent<GameObjectEntity>();
            if (gameObjectEntity != null)
            {
                m_EntityManager = gameObjectEntity.EntityManager;
                m_Entity = gameObjectEntity.Entity;
            }
        }

        private void OnDestroy()
        {
            m_AllColliderHitsInFrame.Clear();
            m_AllMoveEventsInFrame.Clear();

            m_AllColliderHitsInFrame = null;
            m_AllMoveEventsInFrame = null;
            m_CharacterController = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGrounded(int layerMask)
        {
            var worldCenter = m_CachedTransform.position + m_CharacterController.center;
            var lowPoint    = worldCenter - new Vector3(0, m_CharacterController.height * 0.5f, 0);
            var spherePos = lowPoint + new Vector3(0, m_CharacterController.radius - m_CharacterController.skinWidth);

            m_CharacterController.enabled = false;
            
            var result = m_IsGroundForcedThisFrame
                         || m_LastIsGrounded
                         || m_LastCollisionFlags == CollisionFlags.CollidedBelow
                         || Physics.CheckSphere(spherePos, m_CharacterController.radius, layerMask);

            m_CharacterController.enabled = true;
            
            return result;
        }

        /// <summary>
        /// Get move events when an idle motion
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveEvent SmallMove()
        {
            var originPos = m_CachedTransform.position;
            var ev        = MoveBy(new Vector3(0, -(m_CharacterController.minMoveDistance + 0.001f), 0));
            m_CachedTransform.position = originPos;

            return ev;
        }

        /// <summary>
        /// Teleport the controller to a given position, with physics rules (collision, etc...).
        /// If you want to teleport the player without physics rule, just modify the transform position
        /// </summary>
        /// <param name="toPosition"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveEvent MoveTo(Vector3 toPosition)
        {
            return MoveBy(m_CachedTransform.position - toPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MoveEvent MoveBy(Vector3 motion)
        {
            CleanLastFrameEvents();

            var originPosition = m_CachedTransform.position;

            var eventListenerStartIndex = m_AllColliderHitsInFrame.Count;
            m_CharacterController.Move(motion);
            var eventListenerEndIndex = m_AllColliderHitsInFrame.Count;

            m_LastIsGrounded = m_CharacterController.isGrounded;
            m_LastCollisionFlags = m_CharacterController.collisionFlags;
            
            return new MoveEvent()
            {
                CharacterControllerMotor = this,
                Motion                   = motion,
                Origin                   = originPosition,
                Position                 = m_CachedTransform.position,
                EventsStartIndex         = eventListenerStartIndex,
                EventsLength             = eventListenerEndIndex - eventListenerStartIndex
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CleanLastFrameEvents()
        {
            Profiler.BeginSample("CleanLastFrameEvents");
            var frameCount = Time.frameCount;
            if (m_LastCleanFrame < frameCount)
            {
                m_LastCleanFrame = frameCount;

                m_AllMoveEventsInFrame.Clear();
                m_AllColliderHitsInFrame.Clear();

                m_IsGroundForcedThisFrame = false;
            }
            Profiler.EndSample();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Profiler.BeginSample("OnControllerColliderHit");
            m_AllColliderHitsInFrame.Add(hit);
            Profiler.EndSample();
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ControllerColliderHit GetColliderHit(int index)
            {
                if (index > EventsLength)
                    throw new Exception("error todo");
                return CharacterControllerMotor.m_AllColliderHitsInFrame[EventsStartIndex + index];
            }
        }
    }

    public partial class CharacterControllerMotor
    {
        private CharacterController m_CharacterController;
        private Transform           m_CachedTransform;
        private Entity m_Entity;
        private EntityManager m_EntityManager;

        internal List<MoveEvent>             m_AllMoveEventsInFrame    = new List<MoveEvent>();
        internal List<ControllerColliderHit> m_AllColliderHitsInFrame  = new List<ControllerColliderHit>();
        internal bool                        m_IsGroundForcedThisFrame = false;
        internal Vector3                     m_AngleDir;
        internal bool                        m_IsStableOnGround;
        private  int                         m_LastCleanFrame     = 0;
        private  bool                        m_LastIsGrounded     = false;
        private  CollisionFlags              m_LastCollisionFlags = CollisionFlags.None;
    }

    public struct CharacterControllerState : IComponentData
    {
        public byte GroundFlags;
        public byte StableGroundFlags;
        public byte SlideFlags;
        public Vector3 AngleDir;

        public CharacterControllerState(bool isGrounded, bool isStableOnGround, bool isSliding, Vector3 angleDir)
        {
            GroundFlags       = (byte) (isGrounded ? 1 : 0);
            StableGroundFlags = (byte) (isStableOnGround ? 1 : 0);
            SlideFlags        = (byte) (isSliding ? 1 : 0);
            AngleDir = angleDir;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGrounded()
        {
            return GroundFlags == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsStableOnGround()
        {
            return StableGroundFlags == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSliding()
        {
            return SlideFlags == 1;
        }
    }
}