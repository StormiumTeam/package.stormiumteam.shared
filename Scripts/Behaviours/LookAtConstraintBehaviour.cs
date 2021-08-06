using UnityEngine;

namespace package.stormiumteam.shared
{
	[ExecuteAlways]
	public class LookAtConstraintBehaviour : ConstraintBase
	{
		public Vector3   RotOffset;
		public Transform Source;

		protected override void OnUpdate()
		{
			self.LookAt(Source);
			if (RotOffset != Vector3.zero)
				self.Rotate(RotOffset);
		}
	}
}