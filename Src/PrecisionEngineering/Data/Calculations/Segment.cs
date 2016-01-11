using System.Collections.Generic;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Data.Calculations
{
    internal class Segment
    {
        /// <summary>
        /// Calculate the angles between branch and the segment it branches from
        /// </summary>
        /// <param name="netTool"></param>
        /// <param name="measurements">Collection to populate with measurements</param>
        public static void CalculateSegmentBranchAngles(NetToolProxy netTool, ICollection<Measurement> measurements)
        {
            if (netTool.ControlPointsCount < 1)
            {
                return;
            }

            if (netTool.ControlPoints[0].m_segment == 0)
            {
                return;
            }

            var sourceControlPoint = netTool.ControlPoints[0];
            var destControlPoint = netTool.ControlPoints[1];

            if (sourceControlPoint.m_segment == destControlPoint.m_segment)
            {
                return;
            }

            var lineDirection =
                sourceControlPoint.m_position.Flatten().DirectionTo(destControlPoint.m_position.Flatten());

            CalculateAngles(sourceControlPoint.m_position, sourceControlPoint.m_direction.Flatten(), lineDirection,
                measurements);
        }

        public static void CalculateJoinAngles(NetToolProxy netTool, ICollection<Measurement> measurements)
        {
            if (netTool.ControlPointsCount < 1)
            {
                return;
            }

            if (netTool.ControlPoints[netTool.ControlPointsCount].m_segment == 0)
            {
                return;
            }

            var sourceControlPoint = netTool.ControlPoints[netTool.ControlPointsCount - 1];
            var lastControlPoint = netTool.ControlPoints[netTool.ControlPointsCount];

            if (sourceControlPoint.m_segment == lastControlPoint.m_segment)
            {
                return;
            }

            var lineDirection =
                lastControlPoint.m_position.Flatten().DirectionTo(sourceControlPoint.m_position.Flatten());

            Vector3 segmentDirection;
            Vector3 segmentPosition;

            NetManager.instance.m_segments.m_buffer[lastControlPoint.m_segment].GetClosestPositionAndDirection(
                lastControlPoint.m_position, out segmentPosition, out segmentDirection);

            CalculateAngles(segmentPosition, segmentDirection.Flatten(), lineDirection, measurements);
        }

        public static void CalculateAngles(Vector3 anglePosition, Vector3 segmentDirection, Vector3 branchDirection,
            ICollection<Measurement> measurements)
        {
            var angleSize = Vector3.Angle(segmentDirection, branchDirection);
            var angleDirection = Vector3.Normalize(segmentDirection + branchDirection);

            var otherAngleSize = 180f - angleSize;
            var otherAngleDirection = Vector3.Normalize(-segmentDirection + branchDirection);

            if (otherAngleSize < angleSize)
            {
                Util.Swap(ref angleSize, ref otherAngleSize);
                Util.Swap(ref angleDirection, ref otherAngleDirection);
            }

            measurements.Add(new AngleMeasurement(angleSize, anglePosition, angleDirection,
                MeasurementFlags.Primary));

            if (Mathf.Abs(angleSize - 180f) < 0.5f || Mathf.Abs(otherAngleSize - 180f) < 0.5f)
            {
                return;
            }

            measurements.Add(new AngleMeasurement(otherAngleSize, anglePosition, otherAngleDirection,
                MeasurementFlags.Secondary));
        }
    }
}
