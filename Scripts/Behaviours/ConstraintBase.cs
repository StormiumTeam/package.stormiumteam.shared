using UnityEngine;

namespace package.stormiumteam.shared
{
	public abstract class ConstraintBase : MonoBehaviour, ILateUpdate
	{
		public bool IsActive;

		protected Transform self;

		private void OnEnable()
		{
			self = transform;
			/*if (Application.isPlaying)
				UpdateManager.AddLateUpdate(this);*/
		}

		private void OnDisable()
		{
			self = null;
			//UpdateManager.RemoveLateUpdate(this);
		}

		//#if UNITY_EDITOR

		private void LateUpdate()
		{
			/*			if (Application.isPlaying)
							return;
			*/
			if (!IsActive)
				return;
			OnUpdate();
		}

		//#endif

		public void OnLateUpdate()
		{
			if (!IsActive)
				return;
			OnUpdate();
		}

		protected abstract void OnUpdate();
	}
}