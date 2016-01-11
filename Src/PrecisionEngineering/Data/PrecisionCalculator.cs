using System.Collections.Generic;
using PrecisionEngineering.Data.Calculations;
using PrecisionEngineering.Detour;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Data
{
    /// <summary>
    /// The brains of the operation. Delegates out data extraction to various static helper
    /// classes, and compiles a list of measurements from the results.
    /// </summary>
    internal class PrecisionCalculator
    {
        private static readonly ushort[] _segments = new ushort[16];

        private readonly List<Measurement> _measurements = new List<Measurement>();

        public string DebugState = "";

        public IList<Measurement> Measurements
        {
            get { return _measurements; }
        }

        public void Update(NetToolProxy netTool)
        {
            _measurements.Clear();
            DebugState = "";

            if (!netTool.IsEnabled || netTool.Mode == NetTool.Mode.Upgrade)
            {
                return;
            }

            Segment.CalculateSegmentBranchAngles(netTool, _measurements);
            Segment.CalculateJoinAngles(netTool, _measurements);
            Node.CalculateBranchAngles(netTool, _measurements);
            Node.CalculateJoinAngles(netTool, _measurements);
            Guides.CalculateGuideLineAngle(netTool, _measurements);
            Guides.CalculateGuideLineDistance(netTool, _measurements);

            CalculateNearbySegments(netTool, _measurements);

            CalculateControlPointDistances(netTool, _measurements);
            CalculateControlPointAngle(netTool, _measurements);

            CalculateControlPointElevation(netTool, _measurements);
            CalculateCompassAngle(netTool, _measurements);
        }

        /// <summary>
        /// Calculates distances between control points in the NetTool.
        /// </summary>
        private static void CalculateControlPointDistances(NetToolProxy netTool, ICollection<Measurement> measurements)
        {
            if (netTool.ControlPointsCount < 1)
            {
                return;
            }

            for (var i = 1; i < netTool.ControlPointsCount + 1; i++)
            {
                var p1 = netTool.ControlPoints[i - 1].m_position;
                var p2 = netTool.ControlPoints[i].m_position;

                var dist = Vector3.Distance(p1.Flatten(), p2.Flatten());
                var pos = Vector3Extensions.Average(p1, p2);

                measurements.Add(new DistanceMeasurement(dist, pos, true, p1, p2, MeasurementFlags.HideOverlay));
            }
        }

        /// <summary>
        /// If there are 3 control points, calculates the angle between the three points
        /// </summary>
        private static void CalculateControlPointAngle(NetToolProxy netTool, ICollection<Measurement> measurements)
        {
            if (netTool.ControlPointsCount != 2)
            {
                return;
            }

            var p1 = netTool.ControlPoints[0];
            var p2 = netTool.ControlPoints[1];
            var p3 = netTool.ControlPoints[2];

            var d1 = Vector3.Normalize(p1.m_position.Flatten() - p2.m_position.Flatten());
            var d2 = Vector3.Normalize(p3.m_position.Flatten() - p2.m_position.Flatten());

            var angle = Vector3.Angle(d1, d2);
            var angleDirection = Vector3.Normalize(d1 + d2);

            // 180deg angle special case
            if (angleDirection.sqrMagnitude < 0.5f)
            {
                angleDirection = Vector3.Cross(d1, Vector3.up);
            }

            measurements.Add(new AngleMeasurement(angle, p2.m_position, angleDirection,
                MeasurementFlags.Primary | MeasurementFlags.Blueprint));
        }

        /// <summary>
        /// Adds a distance measurement from the last control point to the closest segment.
        /// </summary>
        private static void CalculateNearbySegments(NetToolProxy netTool, ICollection<Measurement> measurements)
        {
            if (netTool.ControlPointsCount == 0)
            {
                return;
            }

            if (netTool.NodePositions.m_size <= 1)
            {
                return;
            }

            if (SnapController.SnappedGuideLine.HasValue)
            {
                return;
            }

            var lastNode = netTool.NodePositions[netTool.NodePositions.m_size - 1];

            int count;

            NetUtil.GetClosestSegments(netTool.NetInfo, lastNode.m_position, _segments, out count);

            if (count == 0)
            {
                return;
            }

            var p1 = lastNode.m_position;

            var minDist = float.MaxValue;
            var p = Vector3.zero;
            var found = false;

            for (var i = 0; i < count; i++)
            {
                if (netTool.ControlPoints[0].m_segment > 0 && _segments[i] == netTool.ControlPoints[0].m_segment)
                {
                    continue;
                }

                var s = NetManager.instance.m_segments.m_buffer[_segments[i]];

                if (!NetUtil.AreSimilarClass(s.Info, netTool.NetInfo))
                {
                    continue;
                }

                var p2 = s.GetClosestPosition(p1);

                // Discard if closest segment position is too close to the source node
                if (Vector3.Distance(netTool.ControlPoints[0].m_position, p2) < Settings.MinimumDistanceMeasure)
                {
                    continue;
                }

                var closestPoint = Util.ClosestPointOnLineSegment(p1, p2, netTool.ControlPoints[0].m_position);

                // Discard if the line contains the start control point
                if (Vector3.Distance(closestPoint, netTool.ControlPoints[0].m_position) <
                    Settings.MinimumDistanceMeasure)
                {
                    continue;
                }

                var direction = p2 - p1;
                var dist = direction.sqrMagnitude;
                direction.Normalize();

                if (dist < minDist)
                {
                    minDist = dist;
                    p = p2;
                    found = true;
                }
            }

            if (found && Mathf.Sqrt(minDist) > Settings.MinimumDistanceMeasure)
            {
                measurements.Add(new DistanceMeasurement(Vector3.Distance(p1, p), Vector3Extensions.Average(p1, p), true,
                    p1, p,
                    MeasurementFlags.Secondary));
            }
        }

        /// <summary>
        /// Calculates the elevation of each control point. The last control point will be marked as primary, others as secondary.
        /// </summary>
        private void CalculateControlPointElevation(NetToolProxy netTool, IList<Measurement> measurements)
        {
            for (var i = 0; i <= netTool.ControlPointsCount; i++)
            {
                // Only display the last control point elevation as a primary measurement
                var flag = i == netTool.ControlPointsCount ? MeasurementFlags.Primary : MeasurementFlags.Secondary;

                var controlPoint = netTool.ControlPoints[i];

                var e = controlPoint.m_elevation;

                var botPos = controlPoint.m_position;
                var topPos = controlPoint.m_position - new Vector3(0, controlPoint.m_elevation, 0);

                measurements.Add(new DistanceMeasurement(e, Vector3Extensions.Average(botPos, topPos), true, botPos,
                    topPos,
                    MeasurementFlags.HideOverlay | MeasurementFlags.Height | flag));
            }
        }

        /// <summary>
        /// Calculates the angle of the first control point relative to compass north.
        /// </summary>
        private void CalculateCompassAngle(NetToolProxy netTool, IList<Measurement> measurements)
        {
            if (netTool.ControlPointsCount < 1)
            {
                return;
            }

            // Ignore if dragging from a node or segment
            if (netTool.ControlPoints[0].m_node != 0 || netTool.ControlPoints[0].m_segment != 0)
            {
                return;
            }

            var direction =
                netTool.ControlPoints[0].m_position.Flatten().DirectionTo(netTool.ControlPoints[1].m_position.Flatten());

            var north = Vector3.forward;

            var angleSize = Vector3.Angle(north, direction);
            var angleDirection = Vector3.Normalize(north + direction);

            if (Mathf.Approximately(angleSize, 180f))
            {
                angleDirection = Vector3.right;
            }

            measurements.Add(new AngleMeasurement(angleSize, netTool.ControlPoints[0].m_position, angleDirection,
                MeasurementFlags.Snap));
        }
    }
}
