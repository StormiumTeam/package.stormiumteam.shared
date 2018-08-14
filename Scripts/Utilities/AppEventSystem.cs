using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine.Experimental.LowLevel;

namespace package.stormiumteam.shared
{
    public static class AppEvent<TEvent> where TEvent : IAppEvent
    {
        private static TEvent[] m_FixedEventList = null;
        
        public static object Caller { get; set; }

        public static List<TEvent> delayList = new List<TEvent>();
        public static List<TEvent> eventList = new List<TEvent>();
        public static List<TEvent> objList = new List<TEvent>();

        public static void Invoke(object caller, Action<TEvent> call)
        {
            foreach (var @event in eventList)
            {
                Caller = caller;
                call(@event);
            }
        }

        public static TEvent[] GetObjEvents()
        {
            if (m_FixedEventList == null
                || m_FixedEventList.Length != eventList.Count)
            {
                m_FixedEventList = eventList.ToArray();
                return m_FixedEventList;
            }

            for (int i = 0; i != m_FixedEventList.Length; i++)
            {
                m_FixedEventList[i] = eventList[i];
            }

            return m_FixedEventList;
        }
    }
        
    public interface IAppEvent
    {
            
    }
    
    public class AppEventSystem : ComponentSystem
    {
        private bool m_HasRan;
        private static List<Type> m_AllExecutables = new List<Type>();
        
        private static MethodInfo m_RemakeMethod;
        private static MethodInfo m_CheckDelayed;

        public static void Register<TEvent>()
            where TEvent : IAppEvent
        {
            m_AllExecutables.Add(typeof(TEvent));
        }

        internal bool HasRanAndPlayerLoopIsCorrect =>
            m_HasRan
            && ScriptBehaviourUpdateOrder.CurrentPlayerLoop.subSystemList != null
            && ScriptBehaviourUpdateOrder.CurrentPlayerLoop.subSystemList.Length > 0
            && ECSWorldLoop.LoopableWorlds.Contains(World);

        private long m_LoopVersion;

        protected override void OnCreateManager(int capacity)
        {
            m_HasRan = false;

            m_RemakeMethod = typeof(AppEventSystem).GetMethod(nameof(Remake));
            m_CheckDelayed = typeof(AppEventSystem).GetMethod(nameof(CheckDelayed));
        }

        public void CheckLoopValidity()
        {
            if (ECSWorldLoop.Version != m_LoopVersion)
            {
                m_LoopVersion = ECSWorldLoop.Version;

                foreach (var executable in m_AllExecutables)
                {
                    var generic = m_RemakeMethod.MakeGenericMethod(executable);
                    generic.Invoke(this, new object[] {});
                }
            }
        }

        protected override void OnStartRunning()
        {
        }

        protected override void OnUpdate()
        {
            // Run delayed actions
            if (!m_HasRan && ScriptBehaviourUpdateOrder.CurrentPlayerLoop.subSystemList != null
                          && ScriptBehaviourUpdateOrder.CurrentPlayerLoop.subSystemList.Length > 0
                          && (ECSWorldLoop.LoopableWorlds.Contains(World) || World == World.Active))
            {
                m_HasRan = true;

                foreach (var executable in m_AllExecutables)
                {
                    var generic = m_CheckDelayed.MakeGenericMethod(executable);
                    generic.Invoke(this, new object[] {});
                }
            }
        }

        protected override void OnDestroyManager()
        {
        }

        public void CheckDelayed<TEvent>()
            where TEvent : IAppEvent
        {
            foreach (var delayed in AppEvent<TEvent>.delayList)
                SubscribeTo<TEvent, TEvent>(delayed);
            
            AppEvent<TEvent>.delayList.Clear();
        }

        public void SubscribeToAll<TObj>(TObj obj)
            where TObj : IAppEvent
        {
            var interfaces = obj.GetType().GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if (!typeof(IAppEvent).IsAssignableFrom(@interface)) continue;
                
                var method  = typeof(AppEventSystem).GetMethod("SubscribeTo");
                var generic = method.MakeGenericMethod(@interface, @interface);
                generic.Invoke(this, new object[] {obj});
            }
        }

        public void SubscribeTo<TSub, TObj>(TObj __obj)
            where TSub : IAppEvent
            where TObj : IAppEvent
        {
            var obj = (TSub)(object) __obj;

            if (!m_AllExecutables.Contains(typeof(TSub)))
            {
                Register<TSub>();
            }
                
            if (!HasRanAndPlayerLoopIsCorrect)
            {
                AppEvent<TSub>.delayList.Add(obj);
                return;
            }

            if (AppEvent<TSub>.eventList.Contains(obj))
                return;   

            var oldList = AppEvent<TSub>.eventList;
            var newList = new List<TSub>();

            var currPlayerLoop = ScriptBehaviourUpdateOrder.CurrentPlayerLoop;
            AddSystem(currPlayerLoop, obj, oldList, newList);

            oldList.Clear();
            AppEvent<TSub>.eventList = newList;
            AppEvent<TSub>.objList.Add(obj);

            foreach (var subObj in AppEvent<TSub>.objList)
            {
                if (!AppEvent<TSub>.eventList.Contains(subObj))
                    AppEvent<TSub>.eventList.Add(subObj);
            }
        }

        public void Remake<TSub>()
            where TSub : IAppEvent
        {
            var oldList = AppEvent<TSub>.eventList;
            var newList = new List<TSub>();

            var currPlayerLoop = ScriptBehaviourUpdateOrder.CurrentPlayerLoop;
            RemakeSystemLoop(currPlayerLoop, oldList, newList);

            oldList.Clear();
            AppEvent<TSub>.eventList = newList;
            
            foreach (var subObj in AppEvent<TSub>.objList)
            {
                if (!AppEvent<TSub>.eventList.Contains(subObj))
                    AppEvent<TSub>.eventList.Add(subObj);
            }
        }
        
        // ...
        private void AddSystem<T>(PlayerLoopSystem loopSystem, T wantedType, List<T> oldList, List<T> newList)
        {
            foreach (var oldManager in oldList)
            {
                if (loopSystem.type == oldManager.GetType())
                {
                    var newManager = World.GetExistingManager(oldManager.GetType());
                    if (newManager != null && !newList.Contains(oldManager)) newList.Add((T) (object) newManager);

                    goto phase2;
                }
            }

            if (!newList.Contains(wantedType) && loopSystem.type == wantedType.GetType())
            {
                newList.Add(wantedType);
            }

            phase2:
            {
                if (loopSystem.subSystemList != null)
                {
                    foreach (var innerLoopSystem in loopSystem.subSystemList)
                        AddSystem(innerLoopSystem, wantedType, oldList, newList);
                }
            }
        }
        
        private void RemakeSystemLoop<T>(PlayerLoopSystem loopSystem, List<T> oldList, List<T> newList)
        {
            foreach (var oldManager in oldList)
            {
                if (loopSystem.type == oldManager.GetType())
                {
                    var newManager = World.GetExistingManager(oldManager.GetType());
                    if (newManager != null && !newList.Contains(oldManager)) newList.Add((T) (object) newManager);

                    goto phase2;
                }
            }

            phase2:
            {
                if (loopSystem.subSystemList != null)
                {
                    foreach (var innerLoopSystem in loopSystem.subSystemList)
                        RemakeSystemLoop(innerLoopSystem, oldList, newList);
                }
            }
        }
    }
}