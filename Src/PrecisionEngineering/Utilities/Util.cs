using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PrecisionEngineering.Utilities
{
	public static class Util
	{

		public static void Swap<T>(ref T one, ref T two)
		{

			var t = one;
			one = two;
			two = t;

		}

		/// <summary>
		/// Returns the closest point on line ab to point p
		/// </summary>
		/// <param name="a">Line point 1</param>
		/// <param name="b">Line point 2</param>
		/// <param name="p">Test point</param>
		/// <source>http://stackoverflow.com/a/3122532</source>
		/// <returns></returns>
		public static Vector3 ClosestPointOnLineSegment(Vector3 a, Vector3 b, Vector3 p)
		{

			var ap = p - a;
			var ab = b - a;

			var apLength = ab.sqrMagnitude;

			var apDotAb = Vector3.Dot(ap, ab);

			var t = apDotAb / apLength;
			t = Mathf.Clamp01(t);

			return a + ab * t;

		}

		/// <summary>
		/// Return the point on the line defined by point <paramref name="a"/> and direction <paramref name="d"/> closest to point <paramref name="p"/>
		/// </summary>
		/// <param name="a">Line origin</param>
		/// <param name="d">Line direction</param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static Vector3 ClosestPointOnLine(Vector3 a, Vector3 d, Vector3 p)
		{

			var b = a + d;

			var ap = p - a;
			var ab = b - a;

			var apLength = ab.sqrMagnitude;

			var apDotAb = Vector3.Dot(ap, ab);

			var t = apDotAb / apLength;

			return a + ab * t;

		}

	}
}
