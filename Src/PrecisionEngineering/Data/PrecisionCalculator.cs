using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrecisionEngineering.Data
{

	class PrecisionCalculator
	{

		public string DebugState = "";

		public IList<Measurement> Measurements
		{
			get { return _measurements; }
		}

		private readonly List<Measurement> _measurements = new List<Measurement>(); 

		public void Update(NetToolProxy netTool)
		{
			
			_measurements.Clear();
			DebugState = "";

			if (!netTool.IsEnabled)
				return;

			//if (netTool.BuildErrors & ToolBase.ToolErrors.VisibleErrors != 0)
			//	return;

			if(netTool.ControlPointsCount > 0 && netTool.ControlPoints[0].m_segment > 0)
				CalculateSegmentBranchAngles(netTool);

			if (netTool.ControlPointsCount > 0 && netTool.ControlPoints[0].m_node > 0)
				CalculateNodeBranchAngles(netTool);

			if (netTool.NodePositions.m_size > 1)
				CalculateDistance(netTool);

		}

		private void CalculateDistance(NetToolProxy netTool)
		{

			float length = 0;

			var prevNodePosition = netTool.NodePositions[0].m_position;
			var avg = prevNodePosition;

			for (var i = 1; i < netTool.NodePositions.m_size; i++) {

				var n = netTool.NodePositions[i];

				length += Vector3.Distance(prevNodePosition, n.m_position);

				prevNodePosition = n.m_position;
				avg += n.m_position;

				//netTool.ControlPoints[0]

			}

			_measurements.Add(new DistanceMeasurement(length, avg*1/netTool.NodePositions.m_size, MeasurementDetail.Primary));

		}

		/// <summary>
		/// Calculate the angles between branch and the segment it branches from
		/// </summary>
		/// <param name="netTool"></param>
		private void CalculateSegmentBranchAngles(NetToolProxy netTool)
		{

			if (netTool.ControlPoints.Count < 1)
				return;

			if (netTool.ControlPoints[0].m_segment == 0)
				return;

			if (netTool.NodePositions.m_size < 2)
				return;

			var sourceSegmentDirection = netTool.ControlPoints[0].m_direction;

			var sourceNode = netTool.NodePositions[0];
			var destNode = netTool.NodePositions[1];

			var lineDirection = sourceNode.m_position.Flatten().DirectionTo(destNode.m_position.Flatten());

			var angleSize = Vector3.Angle(sourceSegmentDirection, lineDirection);
			var angleDirection = Vector3.Normalize(sourceSegmentDirection + lineDirection);

			var otherAngleSize = Vector3.Angle(-sourceSegmentDirection, lineDirection);
			var otherAngleDirection = Vector3.Normalize(-sourceSegmentDirection + lineDirection);

			_measurements.Add(new AngleMeasurement(angleSize, sourceNode.m_position, angleDirection,
				angleSize > otherAngleSize ? MeasurementDetail.Secondary : MeasurementDetail.Primary));

			_measurements.Add(new AngleMeasurement(otherAngleSize, sourceNode.m_position, otherAngleDirection,
				angleSize > otherAngleSize ? MeasurementDetail.Primary : MeasurementDetail.Secondary));

		}

		/// <summary>
		/// Calculate the angles to other segments branching from the same node
		/// </summary>
		/// <param name="netTool"></param>
		private void CalculateNodeBranchAngles(NetToolProxy netTool)
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

			var existingSegments = GetNodeSegments(sourceNode);

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

			_measurements.Add(new AngleMeasurement(angleSize, sourceNode.m_position, angleDirection, MeasurementDetail.Primary));

		}

		private List<NetSegment> _segmentList = new List<NetSegment>(8); 

		private List<NetSegment> GetNodeSegments(NetNode node)
		{

			_segmentList.Clear();

			var list = _segmentList;

			if (node.m_segment0 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment0]);
			if (node.m_segment1 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment1]);
			if (node.m_segment2 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment2]);
			if (node.m_segment3 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment3]);
			if (node.m_segment4 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment4]);
			if (node.m_segment5 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment5]);
			if (node.m_segment6 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment6]);
			if (node.m_segment7 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment7]);

			return list;

		}

	}
}
