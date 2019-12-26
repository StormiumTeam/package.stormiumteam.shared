using UnityEngine;

namespace package.stormiumteam.shared
{
	public static class CameraExtensions
	{
		public static Vector2 GetMin(this Camera camera)
		{
			return (Vector2) camera.transform.position + -GetExtents(camera);
		}

		public static Vector2 GetMax(this Camera camera)
		{
			return (Vector2) camera.transform.position + GetExtents(camera);
		}

		public static Vector2 GetExtents(this Camera camera)
		{
			return new Vector2(camera.orthographicSize * ((float) camera.pixelWidth / camera.pixelHeight), // width
				       camera.orthographicSize) * 2;
		}
	}
}