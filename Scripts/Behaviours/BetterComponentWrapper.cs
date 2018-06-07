using Unity.Entities;
using UnityEngine;

namespace package.guerro.shared
{
    [RequireComponent(typeof(GameObjectEntity))]
    public class BetterComponentWrapper<T> : MonoBehaviour
        where T : struct, IComponentData
    {
        public T Value;

        private GameObjectEntity m_GameObjectEntity;

        private void OnEnable()
        {
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
            RefreshGameObjectEntity();

            var entity = m_GameObjectEntity.Entity;
            var em     = m_GameObjectEntity.EntityManager;

            if (!em.HasComponent<T>(entity))
                em.AddComponentData(entity, Value);
            else
                em.SetComponentData(entity, Value);
        }
#endif

        private void RefreshGameObjectEntity()
        {
            m_GameObjectEntity = ReferencableGameObject.GetComponent<GameObjectEntity>(gameObject);
            if (m_GameObjectEntity == null)
            {
                m_GameObjectEntity = gameObject.AddComponent<GameObjectEntity>();
            }
        }
    }
}