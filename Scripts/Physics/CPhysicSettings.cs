using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using package.stormiumteam.shared;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace package.stormiumteam.shared
{
    /*
     * Public members
     */
    public partial class CPhysicSettings : ComponentSystem
    {
        public static int PhysicInteractionLayerMask;
        public static int PhysicInteractionLayer;
        public static int NoPhysicInteractionLayer;
        
        public const string PhysicInteraction = "PhysicInteraction";
        public const string NoPhysicInteraction = "NoPhysicInteraction";

        /// <summary>
        /// Get the active instance from the world
        /// </summary>
        public static CPhysicSettings Active
            => World.Active.GetOrCreateManager<CPhysicSettings>();

        /// <summary>
        /// Get all the groups from this instance
        /// </summary>
        public ReadOnlyDictionary<string, CPhysicGroup> AllGroups
            => new ReadOnlyDictionary<string, CPhysicGroup>(m_Groups);

        public CPhysicGroup DefaultGroup
        {
            get
            {
                if (m_DefaultGroup == null)
                    m_DefaultGroup = RegisterGroup("default");
                return m_DefaultGroup.Value;
            }
        }

        /*
         * Functions
         */
        /// <summary>
        /// Register a new group
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <returns>The group</returns>
        /// <exception cref="Exception">Thrown when a group with this name already exist</exception>
        public CPhysicGroup RegisterGroup(string name)
        {
            if (m_Groups.ContainsKey(name))
                throw new Exception($"A group with the name `{name}` already exist!");

            return internal_CreateGroup(name);
        }
        
        /// <summary>
        /// Register a new group or return one with the same name
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <returns>The group</returns>
        public CPhysicGroup CreateOrGetGroup(string name)
        {
            CPhysicGroup group;
            return m_Groups.TryGetValue(name, out group) 
                ? group 
                : internal_CreateGroup(name);
        }

        internal CPhysicGroup internal_CreateGroup(string name)
        {
            var group = new CPhysicGroup();
            group.Name = name;
            group.Managed = new CPhysicGroupManaged();
            
            // Create the entity
            var entityManager = World.GetExistingManager<EntityManager>();
            var entity = entityManager.CreateEntity(ComponentType.Create<PhysicGroupData>());
            
            group.Id = entity;

            entityManager.SetSharedComponentData(entity, new PhysicGroupData()
            {
                Group = group,
            });
            
            m_Groups[name] = group;
            
            return group;
        }

        public void SetGroups(GameObject gameObject, CPhysicGroup[] groups)
        {
            var referencableGameObject =
                ReferencableGameObject.GetComponent<ReferencableGameObject>(gameObject);

            var goe = referencableGameObject.GetOrAddComponent<GameObjectEntity>();

            var oldGroups = GetGroups(goe.Entity);
            for (int i = 0; oldGroups.exist && i != oldGroups.buffer.Length; i++)
            {
                var oldGroup = oldGroups.buffer[i];
                if (oldGroup.Target != Entity.Null)
                {
                    var managedGroup = EntityManager.GetSharedComponentData<PhysicGroupData>(oldGroup.Target).Group.Managed;

                    managedGroup.PopGameObject = gameObject;
                }
            }

            SetGroups(goe.Entity, groups);

            for (int i = 0; i != groups.Length; i++)
            {
                groups[i].Managed.InsertGameObject = gameObject;
            }
        }

        public (bool exist, DynamicBuffer<PhysicGroupTargetData> buffer) GetGroups(Entity entity)
        {
            if (!EntityManager.HasComponent<PhysicGroupTargetData>(entity))
                return (false, default(DynamicBuffer<PhysicGroupTargetData>));
            
            return (true, EntityManager.GetBuffer<PhysicGroupTargetData>(entity));
        }

        public void SetGroups(Entity entity, CPhysicGroup[] groups)
        {
            if (!EntityManager.HasComponent<PhysicGroupTargetData>(entity))
                EntityManager.AddBuffer<PhysicGroupTargetData>(entity);

            var buffer = EntityManager.GetBuffer<PhysicGroupTargetData>(entity);
            buffer.Clear();
            
            for (int i = 0; i != groups.Length; i++)
            {
                buffer.Add(new PhysicGroupTargetData(groups[i]));
            }
        }
        
        public void SetDefaultGroup(Entity entity)
        {
            SetGroups(entity, new[] {DefaultGroup});
        }

        public void SetCollision(CPhysicGroup group, bool state)
        {
            Debug.Assert(group.Id != Entity.Null, "group.Id != Entity.Null");
            Debug.Assert(group.Name != null, "group.Name != null");
            
            var managed = m_Groups[group.Name].Managed;

            foreach (var gameObject in managed.GameObjects)
            {
                SetGlobalCollision(gameObject, state);
            }
            
            managed.PushNewVersion();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetGlobalCollision(GameObject gameObject, bool active)
        {
            if (active)
            {
                gameObject.layer = PhysicInteractionLayer;
                return;
            }

            gameObject.layer = NoPhysicInteractionLayer;
        }
    }

    /*
     * Private members
     */
    public partial class CPhysicSettings
    {        
        private Dictionary<string, CPhysicGroup> m_Groups = new Dictionary<string, CPhysicGroup>();
        private CPhysicGroup? m_DefaultGroup;
    }    
    
    /*
     * Implementing abstract members
     */
    public partial class CPhysicSettings
    {
        protected override void OnStartRunning()
        {
            PhysicInteractionLayer = LayerMask.NameToLayer(PhysicInteraction);
            NoPhysicInteractionLayer = LayerMask.NameToLayer(NoPhysicInteraction);
            PhysicInteractionLayerMask = LayerMask.GetMask(PhysicInteraction, "Default");
        }

        protected override void OnUpdate()
        {
            
        }
    }
}