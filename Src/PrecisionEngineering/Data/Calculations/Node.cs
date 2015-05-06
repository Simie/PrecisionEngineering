using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Data.Calculations
{
	static class Node
	{

		/// <summary>
		/// Calculate the angles to other segments branching from the same node
		/// </summary>
		/// <param name="netTool"></param>
		/// <param name="measurements">Collection to populate with measurements</param>
		public static void CalculateBranchAngles(NetToolProxy netTool, ICollection<Measurement> measurements)
		{

			if (netTool.ControlPoints.Count < 1)
				return;

			var sourceNodeId = netTool.ControlPoints[0].m_node;

			if (sourceNodeId == 0)
				return;

			if (netTool.NodePositions.m_size < 2)
				return;

			var firstNewNode = netTool.NodePositions[0];

			CalculateAngles(sourceNodeId, firstNewNode.m_direction, measurements);

		}

		public static void CalculateAngles(ushort nodeId, Vector3 direction, ICollection<Measurement> measurements)
		{

			direction = direction.Flatten();

			var node = NetManager.instance.m_nodes.m_buffer[nodeId];
			var existingSegments = NetNodeUtility.GetNodeSegmentIds(node);

			if (existingSegments.Count == 0)
				return;

			var nearestLeftAngle = 360f;
			ushort nearestLeftSegmentId = 0;
			var nearestLeftNormal = Vector3.zero;

			var nearestRightAngle = 360f;
			ushort nearestRightSegmentId = 0;
			var nearestRightNormal = Vector3.zero;

			for (var i = 0; i < existingSegments.Count; i++) {

				var s = NetManager.instance.m_segments.m_buffer[existingSegments[i]];

				var d = s.m_startNode == nodeId ? s.m_startDirection : s.m_endDirection;
				d = d.Flatten();

				var angle = GetClockwiseAngleBetween(-d, direction, Vector3.up);

				var leftAngle = 360f - angle;
				var rightAngle = angle;

				var n = Vector3.Normalize(direction + d);

				if ((leftAngle < nearestLeftAngle)) {

					nearestLeftAngle = leftAngle;
					nearestLeftSegmentId = existingSegments[i];
					nearestLeftNormal = Quaternion.AngleAxis(leftAngle*0.5f, Vector3.up)*direction;

				}

				if (rightAngle < nearestRightAngle) {

					nearestRightAngle = rightAngle;
					nearestRightSegmentId = existingSegments[i];
					nearestRightNormal = Quaternion.AngleAxis(rightAngle * -0.5f, Vector3.up) * direction;

				}

			}

			// When both angles are 180, only show the one on the right.
			if (Mathf.Approximately(180f, nearestLeftAngle) && Mathf.Approximately(180f, nearestRightAngle)) {

				measurements.Add(new AngleMeasurement(nearestRightAngle, node.m_position, nearestRightNormal,
					MeasurementFlags.Primary));

				return;

			}

			var leftFlags = nearestRightSegmentId > 0
				? (nearestLeftAngle < nearestRightAngle ? MeasurementFlags.Primary : MeasurementFlags.Secondary)
				: MeasurementFlags.Secondary; 
	
			var rightFlags = nearestLeftSegmentId > 0
				? (nearestRightAngle < nearestLeftAngle ? MeasurementFlags.Primary : MeasurementFlags.Secondary)
				: MeasurementFlags.Secondary; 

			if (nearestLeftSegmentId > 0) {

				measurements.Add(new AngleMeasurement(nearestLeftAngle, node.m_position, nearestLeftNormal, leftFlags));

			}

			if (nearestRightSegmentId > 0) {

				measurements.Add(new AngleMeasurement(nearestRightAngle, node.m_position, nearestRightNormal, rightFlags));

			}
			
		}

		static float GetClockwiseAngleBetween(Vector3 a, Vector3 b, Vector3 n)
		{
			// angle in [0,180]
			float angle = Vector3.Angle(a, b);
			float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

			// angle in [-179,180]
			float signed_angle = angle * sign;

			// angle in [0,360] (not used but included here for completeness)
			float angle360 =  (signed_angle + 180) % 360;

			return angle360;
		}

	}
}
