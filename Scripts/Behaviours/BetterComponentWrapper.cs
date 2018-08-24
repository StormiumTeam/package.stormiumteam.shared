using System.Runtime;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace package.stormiumteam.shared
{
    [RequireComponent(typeof(GameObjectEntity))]
    public class BetterComponentWrapper<T> : MonoBehaviour
        where T : struct, IComponentData
    {
        public T Value;
        
        protected void AddComponentData<TComp>(TComp comp)
            where TComp : struct, IComponentData
        {
            m_GameObjectEntity.EntityManager.AddComponentData(m_GameObjectEntity.Entity, comp);
        }

        protected void AddSharedComponentData<TComp>(TComp comp)
            where TComp : struct, ISharedComponentData
        {
            m_GameObjectEntity.EntityManager.AddSharedComponentData(m_GameObjectEntity.Entity, comp);
        }

        protected virtual void OnUnityAwake()
        {
        }

        private GameObjectEntity m_GameObjectEntity;

        private void Awake()
        {
            RefreshGameObjectEntity();
            
            var entity = m_GameObjectEntity.Entity;
            var em     = m_GameObjectEntity.EntityManager;
            
            if (!em.HasComponent<T>(entity))
                em.AddComponentData(entity, Value);
            else
                Value = em.GetComponentData<T>(entity);

            OnUnityAwake();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
                return;
                
            RefreshGameObjectEntity();

            var entity = m_GameObjectEntity.Entity;
            var em     = m_GameObjectEntity.EntityManager;

            if (!em.HasComponent<T>(entity))
                em.AddComponentData(entity, Value);
            else
                Value = em.GetComponentData<T>(entity);
        }

        private void OnDisable()
        {
            if (m_GameObjectEntity == null)
                return;
            
            var entity = m_GameObjectEntity.Entity;
            var em     = m_GameObjectEntity.EntityManager;
            
            m_GameObjectEntity = null;
            
            if (!Application.isPlaying)
                return;

            if (entity == Entity.Null || em == null)
                return;
            
            em.RemoveComponent<T>(entity);
        }

#if UNITY_EDITOR
        private void LateUpdate()
        {
            var entity = m_GameObjectEntity.Entity;
            var em     = m_GameObjectEntity.EntityManager;
            Value = em.GetComponentData<T>(entity);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;
            
            RefreshGameObjectEntity();

            var entity = m_GameObjectEntity.Entity;
            var em     = m_GameObjectEntity.EntityManager;

            if (em == null)
                return;

            if (!em.HasComponent<T>(entity))
                em.AddComponentData(entity, Value);
            else
                em.SetComponentData(entity, Value);
        }
#endif

        private void RefreshGameObjectEntity()
        {
            if (!Application.isPlaying)
                return;

            m_GameObjectEntity = ReferencableGameObject.GetComponent<GameObjectEntity>(gameObject);
            if ((m_GameObjectEntity = gameObject.GetComponent<GameObjectEntity>()) == null)
            {
                m_GameObjectEntity = gameObject.AddComponent<GameObjectEntity>();
            }

            if (m_GameObjectEntity.Entity == Entity.Null)
            {
                m_GameObjectEntity.enabled = false;
                m_GameObjectEntity.enabled = true;
            }
        }
    }
}