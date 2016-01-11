using UnityEngine;

namespace PrecisionEngineering.Data
{
    internal struct GuideLine
    {
        /// <summary>
        /// Point where the GuideLine originates
        /// </summary>
        public Vector3 Origin;

        /// <summary>
        /// Point where the GuideLine intersects with the current line
        /// </summary>
        public Vector3 Intersect;

        /// <summary>
        /// Width of the segment the line is extrapolated from
        /// </summary>
        public float Width;

        /// <summary>
        /// Distance from intersect to the query point
        /// </summary>
        public float Distance;

        /// <summary>
        /// Direction from Origin to Intersect
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Segment that the GuideLine is extrapolated from
        /// </summary>
        public ushort SourceSegment;

        public GuideLine(Vector3 origin, Vector3 intersect, float width, float distance, ushort segment) : this()
        {
            Origin = origin;
            Intersect = intersect;
            Width = width;
            Distance = distance;
            Direction = origin.DirectionTo(intersect);
            SourceSegment = segment;
        }

        public override string ToString()
        {
            return string.Format("O: {0}, I: {1}\n\t Width: {2}, Distance: {3}", Origin, Intersect, Width, Distance);
        }
    }
}
