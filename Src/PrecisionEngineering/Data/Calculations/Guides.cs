using System.Collections.Generic;
using ColossalFramework.Math;
using PrecisionEngineering.Detour;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Data.Calculations
{
    internal static class Guides
    {
        private static readonly ushort[] SegmentCache = new ushort[Settings.GuideLineQueryCount];
        private static int _segmentCacheCount;

        public static void CalculateGuideLines(NetInfo netInfo, NetTool.ControlPoint startPoint,
            NetTool.ControlPoint endPoint,
            IList<GuideLine> resultList)
        {
            lock (SegmentCache)
            {
                var startPosition = startPoint.m_position;
                var endPosition = endPoint.m_position;

                //var segments = NetManager.instance.m_segments;
                //NetManager.instance.GetClosestSegments(endPosition, SegmentCache, out _segmentCacheCount);

                NetUtil.GetClosestSegments(netInfo, endPosition, SegmentCache, out _segmentCacheCount);
                SnapController.DebugPrint = string.Format("Closest Segment Count: {0}", _segmentCacheCount);

                var c = _segmentCacheCount;
                for (ushort i = 0; i < c; i++)
                {
                    var segmentId = SegmentCache[i];

                    var s = NetManager.instance.m_segments.m_buffer[segmentId];

                    // Ensure they are part of the same network
                    if (!NetUtil.AreSimilarClass(s.Info, netInfo))
                    {
                        continue;
                    }

                    if (
                        Vector3Extensions.DistanceSquared(
                            NetManager.instance.m_nodes.m_buffer[s.m_startNode].m_position, endPosition) >
                        Settings.MaxGuideLineQueryDistanceSqr)
                    {
                        continue;
                    }

                    // Test the start and end of the segment

                    // Check if the node can branch in the guide direction (angles less than 45deg or so should be discarded)
                    if (CanNodeBranchInDirection(s.m_endNode, s.m_startDirection))
                    {
                        var endNode = NetManager.instance.m_nodes.m_buffer[s.m_endNode];

                        TestLine(s.Info, startPosition, endPosition,
                            endNode.m_position,
                            endNode.m_position + s.m_startDirection.Flatten(),
                            resultList, segmentId, s.m_endNode);
                    }

                    if (CanNodeBranchInDirection(s.m_startNode, s.m_endDirection))
                    {
                        var startNode = NetManager.instance.m_nodes.m_buffer[s.m_startNode];

                        TestLine(s.Info, startPosition, endPosition,
                            startNode.m_position,
                            startNode.m_position + s.m_endDirection.Flatten(),
                            resultList, segmentId, s.m_startNode);
                    }
                }
            }
        }

        private static void TestLine(NetInfo netInfo, Vector3 startPoint, Vector3 endPoint, Vector3 l1, Vector3 l2,
            IList<GuideLine> lines, ushort segmentId = 0, ushort nodeId = 0)
        {
            var p = Util.LineIntersectionPoint(startPoint.xz(), endPoint.xz(), l1.xz(), l2.xz());

            // Lines never meet, discard
            if (!p.HasValue)
            {
                return;
            }

            var intersect = new Vector3(p.Value.x, 0, p.Value.y);

            var intersectDistanceSqr = Vector3Extensions.DistanceSquared(intersect, endPoint.Flatten());

            // Discard if intersect is silly distance away
            if (intersectDistanceSqr > Settings.MaxGuideLineQueryDistanceSqr)
            {
                return;
            }

            var guideDirection = l1.Flatten().DirectionTo(l2.Flatten());
            var intersectDirection = l1.Flatten().DirectionTo(intersect);

            // Ignore guides in the wrong direction
            if (Mathf.Abs(Vector3Extensions.GetSignedAngleBetween(guideDirection, intersectDirection, Vector3.up)) > 90f)
            {
                return;
            }

            intersect.y = TerrainManager.instance.SampleRawHeightSmooth(intersect);

            var intersectDistance = Mathf.Sqrt(intersectDistanceSqr);

            var line = new GuideLine(l1, intersect, netInfo.m_halfWidth*2f, intersectDistance, segmentId);

            int index;

            if (IsDuplicate(lines, line, out index))
            {
                var d = lines[index];

                // If a duplicate, check if it is closer than the duplicate
                if (Vector3Extensions.DistanceSquared(d.Origin, endPoint) >
                    Vector3Extensions.DistanceSquared(l1, endPoint))
                {
                    lines[index] = line;
                }

                return;
            }

            // Check for intersection with existing nodes
            var ra = Vector3.Cross(guideDirection, Vector3.up);
            var quad = new Quad3(l1 + ra, l1 - ra, intersect + ra, intersect - ra);

            if (NetManager.instance.OverlapQuad(Quad2.XZ(quad), 0, 1280f, ItemClass.CollisionType.Undefined, netInfo.m_class.m_layer, nodeId, 0, segmentId))
            {
                return;
            }

            lines.Add(line);
        }

        public static bool IsDuplicate(IList<GuideLine> existingLines, GuideLine newLine, out int index)
        {
            index = -1;
            return false;
            var l = existingLines.Count;

            // Discard if an existing line has the same direction and origin
            for (var i = 0; i < l; i++)
            {
                var existingLine = existingLines[i];

                if (Vector3.Dot(existingLine.Direction.Flatten(), newLine.Direction.Flatten()) < 0.9f)
                {
                    continue;
                }

                if (Vector3Extensions.DistanceSquared(existingLine.Intersect, newLine.Intersect) < 0.5f)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private static bool CanNodeBranchInDirection(ushort nodeId, Vector3 direction)
        {
            var closestSegmentId = NetNodeUtility.GetClosestSegmentId(nodeId, direction);

            var exitDirection = NetNodeUtility.GetSegmentExitDirection(nodeId, closestSegmentId);

            var exitAngle = Vector3Extensions.GetSignedAngleBetween(direction.Flatten(), exitDirection.Flatten(),
                Vector3.up);

            return Mathf.Abs(exitAngle) >= 45f;
        }

        public static void CalculateGuideLineDistance(NetToolProxy netTool, ICollection<Measurement> measurements)
        {
            if (netTool.ControlPointsCount == 0)
            {
                return;
            }

            lock (SnapController.GuideLineLock)
            {
                if (!SnapController.SnappedGuideLine.HasValue)
                {
                    return;
                }

                var guideLine = SnapController.SnappedGuideLine.Value;

                var dist = Vector3.Distance(guideLine.Origin.Flatten(), guideLine.Intersect.Flatten());
                var pos = Vector3Extensions.Average(guideLine.Origin, guideLine.Intersect);

                measurements.Add(new DistanceMeasurement(dist, pos, true, guideLine.Origin, guideLine.Intersect,
                    MeasurementFlags.HideOverlay | MeasurementFlags.Guide));
            }
        }

        public static void CalculateGuideLineAngle(NetToolProxy netTool, IList<Measurement> measurements)
        {
            if (netTool.ControlPointsCount == 0)
            {
                return;
            }

            lock (SnapController.GuideLineLock)
            {
                if (!SnapController.SnappedGuideLine.HasValue)
                {
                    return;
                }

                var lastControlPoint = netTool.ControlPoints[netTool.ControlPointsCount];
                var guideLine = SnapController.SnappedGuideLine.Value;

                var incomingDirection = lastControlPoint.m_direction;

                CalculateAngles(guideLine.Intersect, guideLine.Direction, -incomingDirection, measurements);
            }
        }

        private static void CalculateAngles(Vector3 anglePosition, Vector3 guideDirection, Vector3 incomingDirection,
            ICollection<Measurement> measurements)
        {
            guideDirection = guideDirection.Flatten().normalized;
            incomingDirection = incomingDirection.Flatten().normalized;

            var angleSize = Vector3.Angle(guideDirection, incomingDirection);
            var angleDirection = Vector3.Normalize(guideDirection + incomingDirection);

            var otherAngleSize = 180f - angleSize;
            var otherAngleDirection = Vector3.Normalize(-guideDirection + incomingDirection);

            if (otherAngleSize < angleSize)
            {
                Util.Swap(ref angleSize, ref otherAngleSize);
                Util.Swap(ref angleDirection, ref otherAngleDirection);
            }

            measurements.Add(new AngleMeasurement(angleSize, anglePosition, angleDirection,
                MeasurementFlags.Primary | MeasurementFlags.Guide));

            if (Mathf.Abs(angleSize - 180f) < 0.5f || Mathf.Abs(otherAngleSize - 180f) < 0.5f)
            {
                return;
            }

            measurements.Add(new AngleMeasurement(otherAngleSize, anglePosition, otherAngleDirection,
                MeasurementFlags.Secondary | MeasurementFlags.Guide));
        }
    }
}
