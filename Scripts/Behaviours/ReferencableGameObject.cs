using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace package.guerro.shared
{
    public class ReferencableGameObject : MonoBehaviour
    {
        private static FastDictionary<int, ReferencableGameObject> s_GameObjects =
            new FastDictionary<int, ReferencableGameObject>();

        private List<Component> m_Components = new List<Component>();

        public ReadOnlyCollection<Component> Components => new ReadOnlyCollection<Component>(m_Components);

        private bool       m_WasCreated = false;
        private GameObject m_fastPathGameObject;

        private void OnCreate()
        {
            if (m_WasCreated)
                return;
            m_WasCreated         = true;
            m_fastPathGameObject = gameObject;

            m_Components = new List<Component>(GetComponents<Component>());

            RegisterReferencable();
        }

        private void Awake()
        {
            OnCreate();
        }

        private void OnEnable()
        {
            OnCreate();
        }

        private void OnDisable()
        {
            m_Components.Clear();
            m_Components = null;
        }

        public T GetComponentFast<T>()
            where T : Component
        {
            var length = Components.Count;
            for (int i = 0; i != length; i++)
            {
                if (Components[i].GetType() == typeof(T))
                    return (T) Components[i];
            }

            return null;
        }

        private void RegisterReferencable()
        {
            s_GameObjects[gameObject.GetInstanceID()] = this;
        }

        public new static T GetComponent<T>(GameObject gameObject, bool createReferencable = true)
            where T : Component
        {
            ReferencableGameObject referencableGameObject;
            if (!Application.isPlaying)
            {
                referencableGameObject = gameObject.GetComponent<ReferencableGameObject>();
                if (referencableGameObject == null && createReferencable)
                    gameObject.AddComponent<ReferencableGameObject>();
                return referencableGameObject == null
                    ? gameObject.GetComponent<T>()
                    : referencableGameObject.GetComponentFast<T>();
            }

            s_GameObjects.FastTryGet(gameObject.GetInstanceID(), out referencableGameObject);
            if (referencableGameObject == null)
            {
                referencableGameObject = gameObject.GetComponent<ReferencableGameObject>();
                if (referencableGameObject == null)
                {
                    if (createReferencable)
                    {
                        referencableGameObject = gameObject.AddComponent<ReferencableGameObject>();
                    }
                    else
                        return gameObject.GetComponent<T>();
                }
            }

            return referencableGameObject.GetComponentFast<T>();
        }
        
        public new static T GetComponent<T>(int referenceId, bool createReferencable = true)
            where T : Component
        {
            ReferencableGameObject referencableGameObject;
            s_GameObjects.FastTryGet(referenceId, out referencableGameObject);
            if (referencableGameObject == null)
            {
                Debug.LogError("Not found");
                return null;
            }

            return referencableGameObject.GetComponentFast<T>();
        }

        public static GameObject FromId(int collidedGameObject)
        {
            ReferencableGameObject referencableGameObject;
            s_GameObjects.FastTryGet(collidedGameObject, out referencableGameObject);
            return referencableGameObject.m_fastPathGameObject;
        }
    }
}