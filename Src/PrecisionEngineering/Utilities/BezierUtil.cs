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
                a = new Vector3(r*Mathf.Cos(a1), 0, r*Mathf.Sin(a1)),
                b = new Vector3(x2*cos_ar - y2*sin_ar, 0, x2*sin_ar + y2*cos_ar),
                c = new Vector3(x3*cos_ar - y3*sin_ar, 0, x3*sin_ar + y3*cos_ar),
                d = new Vector3(r*Mathf.Cos(a2), 0, r*Mathf.Sin(a2))
            };
        }
    }
}
