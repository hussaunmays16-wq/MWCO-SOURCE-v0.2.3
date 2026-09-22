using System;
using UnityEngine;

namespace MWCO.Math
{
	public static class Vector3Extensions
	{
		public static Vector3 Interpolate(this Vector3 start, Vector3 end, float T = 0.015f)
		{
			float num = (0f + Time.deltaTime) / T;
			return Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, num));
		}
	}
}
