using UnityEngine;

namespace package.stormiumteam.shared
{
	[ExecuteAlways]
	public class PositionConstraintBehaviour : ConstraintBase
	{
		public Vector3   Offset;
		public Vector3   WorldOffset;
		public Transform Source;

		protected override void OnUpdate()
		{
			self.position = Source.TransformPoint(Offset) + WorldOffset;
		}
	}
}