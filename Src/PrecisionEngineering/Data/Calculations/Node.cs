using System;
using System.Collections.Generic;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Data.Calculations
{
    internal static class Node
    {
        /// <summary>
        /// Calculate the angles to other segments branching from the same node
        /// </summary>
        /// <param name="netTool"></param>
        /// <param name="measurements">Collection to populate with measurements</param>
        public static void CalculateBranchAngles(NetToolProxy netTool, ICollection<Measurement> measurements)
        {
            if (netTool.ControlPoints.Count < 1)
            {
                return;
            }

            if (netTool.ControlPoints[0].m_node == netTool.ControlPoints[1].m_node)
            {
                return;
            }

            var sourceNodeId = netTool.ControlPoints[0].m_node;

            if (sourceNodeId == 0)
            {
                return;
            }

            if (netTool.ControlPointsCount < 1)
            {
                return;
            }

            var firstNewNode = netTool.NodePositions[0];

            var direction = firstNewNode.m_direction;

            if (netTool.NodePositions.m_size <= 1)
            {
                direction = netTool.ControlPoints[1].m_direction;
            }

            CalculateAngles(sourceNodeId, direction, measurements);
        }

        /// <summary>
        /// Calculate the angles to other segments branching from the same node
        /// </summary>
        /// <param name="netTool"></param>
        /// <param name="measurements">Collection to populate with measurements</param>
        public static void CalculateJoinAngles(NetToolProxy netTool, ICollection<Measurement> measurements)
        {
            if (netTool.ControlPoints.Count < 1)
            {
                return;
            }

            if (netTool.ControlPoints[0].m_node == netTool.ControlPoints[1].m_node)
            {
                return;
            }

            var controlPoint = netTool.ControlPoints[netTool.ControlPointsCount];

            var destNodeId = controlPoint.m_node;

            if (destNodeId == 0)
            {
                return;
            }

            if (netTool.NodePositions.m_size < 2)
            {
                return;
            }

            //var lastNode = netTool.NodePositions[netTool.NodePositions.m_size-1];

            CalculateAngles(destNodeId, -controlPoint.m_direction, measurements);
        }

        public static void CalculateAngles(ushort nodeId, Vector3 direction, ICollection<Measurement> measurements)
        {
            direction = direction.Flatten();

            var node = NetManager.instance.m_nodes.m_buffer[nodeId];
            var existingSegments = NetNodeUtility.GetNodeSegmentIds(node);

            if (existingSegments.Count == 0)
            {
                return;
            }

            var nearestLeftAngle = 360f;
            ushort nearestLeftSegmentId = 0;
            var nearestLeftNormal = Vector3.zero;

            var nearestRightAngle = 360f;
            ushort nearestRightSegmentId = 0;
            var nearestRightNormal = Vector3.zero;

            for (var i = 0; i < existingSegments.Count; i++)
            {
                var s = NetManager.instance.m_segments.m_buffer[existingSegments[i]];

                var d = s.m_startNode == nodeId ? s.m_startDirection : s.m_endDirection;
                d = d.Flatten();

                var angle = Vector3Extensions.GetClockwiseAngleBetween(-d, direction, Vector3.up);

                var leftAngle = 360f - angle;
                var rightAngle = angle;

                var n = Vector3.Normalize(direction + d);

                if (leftAngle < nearestLeftAngle)
                {
                    nearestLeftAngle = leftAngle;
                    nearestLeftSegmentId = existingSegments[i];
                    nearestLeftNormal = Quaternion.AngleAxis(leftAngle*0.5f, Vector3.up)*direction;
                }

                if (rightAngle < nearestRightAngle)
                {
                    nearestRightAngle = rightAngle;
                    nearestRightSegmentId = existingSegments[i];
                    nearestRightNormal = Quaternion.AngleAxis(rightAngle*-0.5f, Vector3.up)*direction;
                }
            }

            // When both angles are 180, only show the one on the right.
            if (Math.Abs(nearestLeftAngle - 180f) < 0.25f && Math.Abs(nearestRightAngle - 180f) < 0.25f)
            {
                measurements.Add(new AngleMeasurement(nearestRightAngle, node.m_position, nearestRightNormal,
                    MeasurementFlags.Primary));

                return;
            }

            var leftFlags = nearestRightSegmentId > 0
                ? (nearestLeftAngle < nearestRightAngle ? MeasurementFlags.Primary : MeasurementFlags.Secondary)
                : MeasurementFlags.Secondary;

            var rightFlags = nearestLeftSegmentId > 0
                ? (nearestRightAngle <= nearestLeftAngle ? MeasurementFlags.Primary : MeasurementFlags.Secondary)
                : MeasurementFlags.Secondary;

            if (nearestLeftSegmentId > 0)
            {
                measurements.Add(new AngleMeasurement(nearestLeftAngle, node.m_position, nearestLeftNormal, leftFlags));
            }

            if (nearestRightSegmentId > 0)
            {
                measurements.Add(new AngleMeasurement(nearestRightAngle, node.m_position, nearestRightNormal, rightFlags));
            }
        }
    }
}
