using System;
using System.Collections.Generic;
using System.Linq;
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

			var sourceNode = NetManager.instance.m_nodes.m_buffer[sourceNodeId];
			var firstNewNode = netTool.NodePositions[0];

			var existingSegments = NetNodeUtility.GetNodeSegments(sourceNode);

			if (existingSegments.Count == 0)
				return;

			// For now, find the segment that closest matches the branch
			var nearestSegment = existingSegments.OrderByDescending(
				s =>
					Vector3.Dot(firstNewNode.m_direction,
						(s.m_startNode == sourceNodeId ? s.m_startDirection : s.m_endDirection)))
			                                     .FirstOrDefault();

			var nearestSegmentDirection = nearestSegment.m_startNode == sourceNodeId ? nearestSegment.m_startDirection : nearestSegment.m_endDirection;

			var angleSize = Vector3.Angle(firstNewNode.m_direction, nearestSegmentDirection);
			var angleDirection = Vector3.Normalize(firstNewNode.m_direction + nearestSegmentDirection);

			measurements.Add(new AngleMeasurement(angleSize, sourceNode.m_position, angleDirection, MeasurementFlags.Primary));

		}

	}
}
