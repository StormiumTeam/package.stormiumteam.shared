using System;
using System.Collections.Generic;
using UnityEngine;

namespace package.stormiumteam.shared
{
	public interface IUpdate
	{
		void OnUpdate();
	}

	public interface ILateUpdate
	{
		void OnLateUpdate();
	}

	public class UpdateManager : MonoBehaviour
	{
		private static UpdateManager __instance;
		
		public static UpdateManager instance
		{
			get
			{
				if (__instance == null)
				{
					var go = new GameObject("ST::UpdateManager", typeof(UpdateManager));
					__instance = go.GetComponent<UpdateManager>();
				}

				return __instance;
			}
		}

		private void Awake()
		{
			if (__instance != null && __instance != this)
				Destroy(gameObject);
			
			__instance = this;
			DontDestroyOnLoad(this);
		}

		private void OnDestroy()
		{
			updateList.Clear();
			lateUpdateList.Clear();
		}

		private void Update()
		{
			var length = updateList.Count;
			for (var i = 0; i != length; i++)
				updateList[i]();
		}

		private void LateUpdate()
		{
			var length = lateUpdateList.Count;
			for (var i = 0; i != length; i++)
				lateUpdateList[i]();	
		}

		private static List<Action> updateList     = new List<Action>();
		private static List<Action> lateUpdateList = new List<Action>();

		public static void AddUpdate(IUpdate update)
		{
			var _ = instance;
			if (!updateList.Contains(update.OnUpdate))
				updateList.Add(update.OnUpdate);
		}

		public static void AddLateUpdate(ILateUpdate update)
		{
			var _ = instance;
			if (!lateUpdateList.Contains(update.OnLateUpdate))
				lateUpdateList.Add(update.OnLateUpdate);
		}

		public static void RemoveUpdate(IUpdate update)
		{
			var _ = instance;
			while (updateList.Remove(update.OnUpdate))
			{}
		}

		public static void RemoveLateUpdate(ILateUpdate update)
		{
			var _ = instance;
			while (lateUpdateList.Remove(update.OnLateUpdate))
			{}
		}
	}
}