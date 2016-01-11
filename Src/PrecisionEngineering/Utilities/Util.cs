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

            var t = apDotAb/apLength;
            t = Mathf.Clamp01(t);

            return a + ab*t;
        }

        /// <summary>
        /// Return the point on the line defined by point <paramref name="a" /> and direction <paramref name="d" /> closest to
        /// point <paramref name="p" />
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

            var t = apDotAb/apLength;

            return a + ab*t;
        }

        public static Vector2? LineIntersectionPoint(Vector2 ps1, Vector2 pe1, Vector2 ps2,
            Vector2 pe2)
        {
            // Get A,B,C of first line - points : ps1 to pe1
            var a1 = pe1.y - ps1.y;
            var b1 = ps1.x - pe1.x;
            var c1 = a1*ps1.x + b1*ps1.y;

            // Get A,B,C of second line - points : ps2 to pe2
            var a2 = pe2.y - ps2.y;
            var b2 = ps2.x - pe2.x;
            var c2 = a2*ps2.x + b2*ps2.y;

            // Get delta and check if the lines are parallel
            var delta = a1*b2 - a2*b1;
            if (Mathf.Approximately(delta, 0))
            {
                return null;
            }

            // now return the Vector2 intersection point
            return new Vector2(
                (b2*c1 - b1*c2)/delta,
                (a1*c2 - a2*c1)/delta
                );
        }
    }
}
