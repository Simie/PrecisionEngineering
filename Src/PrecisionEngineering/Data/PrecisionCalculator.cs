using System.Collections.Generic;
using System.Linq;
using PrecisionEngineering.Utilities;
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

			if (netTool.NodePositions.m_size > 1)
				CalculateNearbyNodes(netTool);

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

			}

			var d = new DistanceMeasurement(length, avg*1/netTool.NodePositions.m_size, true, netTool.NodePositions[0].m_position,
				netTool.NodePositions[netTool.NodePositions.m_size - 1].m_position, MeasurementFlags.Primary | MeasurementFlags.HideOverlay);

			_measurements.Add(d);

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
				angleSize > otherAngleSize ? MeasurementFlags.Secondary : MeasurementFlags.Primary));

			_measurements.Add(new AngleMeasurement(otherAngleSize, sourceNode.m_position, otherAngleDirection,
				angleSize > otherAngleSize ? MeasurementFlags.Primary : MeasurementFlags.Secondary));

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

			_measurements.Add(new AngleMeasurement(angleSize, sourceNode.m_position, angleDirection, MeasurementFlags.Primary));

		}

		private readonly ushort[] _segments = new ushort[16];

		private void CalculateNearbyNodes(NetToolProxy netTool)
		{

			var lastNode = netTool.NodePositions[netTool.NodePositions.m_size - 1];

			int count;

			NetManager.instance.GetClosestSegments(lastNode.m_position, _segments, out count);

			if (count == 0)
				return;

			List<ushort> sourceNodeConnectedSegmentIds = null;

			if (netTool.ControlPoints[0].m_node > 0) {

				sourceNodeConnectedSegmentIds =
					NetNodeUtility.GetNodeSegmentIds(NetManager.instance.m_nodes.m_buffer[netTool.ControlPoints[0].m_node]);

			}

			var p1 = lastNode.m_position;

			var minDist = float.MaxValue;
			var p = Vector3.zero;
			var found = false;

			for (var i = 0; i < count; i++) {

				// Skip segments attached to the node we are building from
				if (sourceNodeConnectedSegmentIds != null && sourceNodeConnectedSegmentIds.Contains(_segments[i]))
					continue;

				var s = NetManager.instance.m_segments.m_buffer[_segments[i]];

				var p2 = s.GetClosestPosition(p1);

				var dist = Vector3.Distance(p1, p2);

				if (dist < minDist) {
					minDist = dist;
					p = p2;
					found = true;
				}

			}

			if (found) {

				_measurements.Add(new DistanceMeasurement(Vector3.Distance(p1, p), Vector3Extensions.Average(p1, p), true, p1, p,
					MeasurementFlags.Secondary));

			}

		}


	}
}
