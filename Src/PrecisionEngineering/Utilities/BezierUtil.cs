using System.Collections.Generic;
using ColossalFramework.Math;
using UnityEngine;

namespace PrecisionEngineering.Utilities
{
    internal static class BezierUtil
    {
        private const float EPSILON = 0.00001f; // Roughly 1/1000th of a degree, see below

        public static IList<Bezier3> CreateArc(Vector3 position, float radius, float startAngle, float endAngle)
        {
            var arcs = CreateArc(radius, Mathf.Deg2Rad*startAngle, Mathf.Deg2Rad*endAngle);

            for (var i = 0; i < arcs.Count; i++)
            {
                var b = arcs[i];
                b.a += position;
                b.b += position;
                b.c += position;
                b.d += position;

                arcs[i] = b;
            }

            return arcs;
        }

        /**
		*  Return a array of objects that represent bezier curves which approximate the 
		*  circular arc centered at the origin, from startAngle to endAngle (radians) with 
		*  the specified radius.
		*  
		*  Each bezier curve is an object with four points, where x1,y1 and 
		*  x4,y4 are the arc's end points and x2,y2 and x3,y3 are the cubic bezier's 
		*  control points.
		*/

        private static IList<Bezier3> CreateArc(float radius, float startAngle, float endAngle)
        {
            // normalize startAngle, endAngle to [-2PI, 2PI]

            const float twoPI = Mathf.PI*2f;
            const float piOverTwo = Mathf.PI/2.0f;

            startAngle = startAngle%twoPI;
            endAngle = endAngle%twoPI;

            // Compute the sequence of arc curves, up to PI/2 at a time.  Total arc angle
            // is less than 2PI.

            var result = new List<Bezier3>();

            var sgn = startAngle < endAngle ? 1 : -1;

            var a1 = startAngle;

            for (var totalAngle = Mathf.Min(twoPI, Mathf.Abs(endAngle - startAngle)); totalAngle > EPSILON;)
            {
                var a2 = a1 + sgn*Mathf.Min(totalAngle, piOverTwo);
                result.Add(CreateSmallArc(radius, a1, a2));
                totalAngle -= Mathf.Abs(a2 - a1);
                a1 = a2;
            }

            return result;
        }

        /**
		*  Cubic bezier approximation of a circular arc centered at the origin, 
		*  from (radians) a1 to a2, where a2-a1 < pi/2.  The arc's radius is r.
		* 
		*  Returns an object with four points, where x1,y1 and x4,y4 are the arc's end points
		*  and x2,y2 and x3,y3 are the cubic bezier's control points.
		* 
		*  This algorithm is based on the approach described in:
		*  A. Riškus, "Approximation of a Cubic Bezier Curve by Circular Arcs and Vice Versa," 
		*  Information Technology and Control, 35(4), 2006 pp. 371-378.
		*/

        private static Bezier3 CreateSmallArc(float r, float a1, float a2)
        {
            // Compute all four points for an arc that subtends the same total angle
            // but is centered on the X-axis

            var a = (a2 - a1)/2.0f; // 

            var x4 = r*Mathf.Cos(a);
            var y4 = r*Mathf.Sin(a);
            var x1 = x4;
            var y1 = -y4;

            const float k = 0.5522847498f;
            var f = k*Mathf.Tan(a);

            var x2 = x1 + f*y4;
            var y2 = y1 + f*x4;
            var x3 = x2;
            var y3 = -y2;

            // Find the arc points actual locations by computing x1,y1 and x4,y4 
            // and rotating the control points by a + a1

            var ar = a + a1;
            var cos_ar = Mathf.Cos(ar);
            var sin_ar = Mathf.Sin(ar);

            return new Bezier3
            {
                a = new Vector3(r * Mathf.Cos(a1), 0, r * Mathf.Sin(a1)),
                b = new Vector3(x2 * cos_ar - y2 * sin_ar, 0, x2 * sin_ar + y2 * cos_ar),
                c = new Vector3(x3 * cos_ar - y3 * sin_ar, 0, x3 * sin_ar + y3 * cos_ar),
                d = new Vector3(r * Mathf.Cos(a2), 0, r * Mathf.Sin(a2))
            };
        }

        public static Bezier3 CreateNetworkCurve(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            //Beziers take 4 points, not 3, so we make a crude approximation of the two midle controll points here
            var c1 = (p2 - p1) * 0.552f + p1;
            var c2 = (p2 - p3) * 0.552f + p3;

            return new Bezier3(p1, c1, c2, p3);
        }

        public static float FindCurvatureRadius(Bezier3 bezier, float t)
        {
            const float delta = 0.01f;
            float t1, t2, t3;
            if(t < delta*2)
            {
                t1 = t;
                t2 = t + delta;
                t3 = t + delta * 2;
            }
            else if (t > 1 - delta * 2)
            {
                t1 = t;
                t2 = t - delta;
                t3 = t - delta * 2;
            }
            else
            {
                t1 = t - delta;
                t2 = t;
                t3 = t + delta;
            }
            var p1 = bezier.Position(t1).Flatten();
            var p2 = bezier.Position(t2).Flatten();
            var p3 = bezier.Position(t3).Flatten();

            FindCircle(p1, p2, p3, out var center, out var radius);

            //If the curve radius is big enough, we may as well ignore it.
            if (radius > 10000)
                radius = float.NaN;

            return radius;
        }
        // Find a circle through the three points.
        private static void FindCircle(Vector3 a, Vector3 b, Vector3 c,
            out Vector3 center, out float radius)
        {
            // Get the perpendicular bisector of (x1, y1) and (x2, y2).
            float x1 = (b.x + a.x) / 2;
            float y1 = (b.z + a.z) / 2;
            float dy1 = b.x - a.x;
            float dx1 = -(b.z - a.z);

            // Get the perpendicular bisector of (x2, y2) and (x3, y3).
            float x2 = (c.x + b.x) / 2;
            float y2 = (c.z + b.z) / 2;
            float dy2 = c.x - b.x;
            float dx2 = -(c.z - b.z);

            // See where the lines intersect.
            bool lines_intersect, segments_intersect;
            Vector3 intersection, close1, close2;
            FindIntersection(
                new Vector3(x1, 0, y1), new Vector3(x1 + dx1, 0, y1 + dy1),
                new Vector3(x2, 0, y2), new Vector3(x2 + dx2, 0, y2 + dy2),
                out lines_intersect, out segments_intersect,
                out intersection, out close1, out close2);
            if (!lines_intersect)
            {
                center = new Vector3(0, 0, 0);
                radius = float.NaN;
            }
            else
            {
                center = intersection;
                float dx = center.x - a.x;
                float dy = center.z - a.z;
                radius = (float)Mathf.Sqrt(dx * dx + dy * dy);
            }
        }
        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        private static void FindIntersection(
            Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
            out bool lines_intersect, out bool segments_intersect,
            out Vector3 intersection,
            out Vector3 close_p1, out Vector3 close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.x - p1.x;
            float dy12 = p2.z - p1.z;
            float dx34 = p4.x - p3.x;
            float dy34 = p4.z - p3.z;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.x - p3.x) * dy34 + (p3.z - p1.z) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new Vector3(float.NaN, float.NaN, float.NaN);
                close_p1 = new Vector3(float.NaN, float.NaN, float.NaN);
                close_p2 = new Vector3(float.NaN, float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.x - p1.x) * dy12 + (p1.z - p3.z) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new Vector3(p1.x + dx12 * t1, 0, p1.z + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new Vector3(p1.x + dx12 * t1, 0, p1.z + dy12 * t1);
            close_p2 = new Vector3(p3.x + dx34 * t2, 0, p3.z + dy34 * t2);
        }
    }
}
