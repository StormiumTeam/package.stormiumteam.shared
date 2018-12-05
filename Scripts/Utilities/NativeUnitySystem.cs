using System;
using package.stormiumteam.shared;
using Unity.Entities;
using UnityEngine;

namespace package.stormiumteam.shared
{
    public class NativeUnitySystem : MonoBehaviour
    {
        private static NativeUnitySystem m_Instance;

        private AppEventSystem m_AppEventSystem;

        private void Awake()
        {
            if (m_Instance && m_Instance != this)
                Destroy(gameObject);

            m_Instance = this;
            m_AppEventSystem = World.Active.GetOrCreateManager<AppEventSystem>();
            
            DontDestroyOnLoad(gameObject);
        }
        
        void OnGUI()
        {
            var eventList = AppEvent<INativeEventOnGUI>.GetObjEvents();
            foreach (var obj in eventList)
            {
                try
                {
                    obj.NativeOnGUI();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

    public interface INativeEventOnGUI : IAppEvent
    {
        void NativeOnGUI();
    }
}