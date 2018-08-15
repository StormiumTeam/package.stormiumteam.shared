using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using package.stormiumteam.shared;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    /*
     * Public members
     */
    public partial class CPhysicSettings : ComponentSystem
    {
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
        public CPhysicGroup RegisterOrCreateGroup(string name)
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

            m_Groups[name] = group;
            
            // Create the entity
            var entityManager = World.GetExistingManager<EntityManager>();
            var entity = entityManager.CreateEntity(ComponentType.Create<PhysicGroupData>());

            entityManager.SetSharedComponentData(entity, new PhysicGroupData()
            {
                Group = group
            });
            
            group.Id = entity;
            
            return group;
        }

        public void SetGroup(GameObject gameObject, CPhysicGroup group)
        {
            var referencableGameObject = 
                ReferencableGameObject.GetComponent<ReferencableGameObject>(gameObject);
            
            var goe = referencableGameObject.GetOrAddComponent<GameObjectEntity>();
            
            SetGroup(goe.Entity, group);
        }

        public void SetGroup(Entity entity, CPhysicGroup group)
        {
            if (!EntityManager.HasComponent<PhysicGroupTargetData>(entity))
                EntityManager.AddComponentData(entity, new PhysicGroupTargetData(group));
            else
                EntityManager.SetComponentData(entity, new PhysicGroupTargetData(group));
        }
        
        public void SetDefaultGroup(Entity entity)
        {
            if (!EntityManager.HasComponent<PhysicGroupTargetData>(entity))
                EntityManager.AddComponentData(entity, new PhysicGroupTargetData(DefaultGroup));
            else
                EntityManager.SetComponentData(entity, new PhysicGroupTargetData(DefaultGroup));
        }

        public void SetCollision(CPhysicGroup group, bool state)
        {
            
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
        protected override void OnUpdate()
        {
            
        }
    }
}