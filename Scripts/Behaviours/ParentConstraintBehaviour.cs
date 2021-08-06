using UnityEngine;

namespace package.stormiumteam.shared
{
	[ExecuteAlways]
	public class ParentConstraintBehaviour : ConstraintBase
	{
		public Vector3   PosOffset;
		public Vector3   RotOffset;
		public Transform Source;

		protected override void OnUpdate()
		{
			if (PosOffset == Vector3.zero)
			{
				self.SetPositionAndRotation(Source.position, Source.rotation * Quaternion.Euler(RotOffset));
				return;
			}

			self.SetPositionAndRotation(Source.TransformPoint(PosOffset), Source.rotation * Quaternion.Euler(RotOffset));
		}
	}
}