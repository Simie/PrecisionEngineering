using System.Collections.Generic;
using System.Linq;
using PrecisionEngineering.Data.Calculations;
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

			Segment.CalculateSegmentBranchAngles(netTool, _measurements);
			Segment.CalculateJoinAngles(netTool, _measurements);
			Node.CalculateBranchAngles(netTool, _measurements);
			Node.CalculateJoinAngles(netTool, _measurements);

			//CalculateNodePositionDistance(netTool, _measurements);
			CalculateNearbySegments(netTool, _measurements);

			CalculateControlPointDistances(netTool, _measurements);
			CalculateControlPointAngle(netTool, _measurements);

		}

		private static void CalculateControlPointDistances(NetToolProxy netTool, ICollection<Measurement> measurements)
		{

			if (netTool.ControlPointsCount < 1)
				return;

			for (var i = 1; i < netTool.ControlPointsCount+1; i++) {

				var p1 = netTool.ControlPoints[i - 1].m_position;
				var p2 = netTool.ControlPoints[i].m_position;

				var dist = Vector3.Distance(p1, p2);
				var pos = Vector3Extensions.Average(p1, p2);

				measurements.Add(new DistanceMeasurement(dist, pos, true, p1, p2, MeasurementFlags.HideOverlay));

			}

		}

		private static void CalculateControlPointAngle(NetToolProxy netTool, ICollection<Measurement> measurements)
		{

			if (netTool.ControlPointsCount < 2)
				return;

			var p1 = netTool.ControlPoints[0];
			var p2 = netTool.ControlPoints[1];
			var p3 = netTool.ControlPoints[2];

			var d1 = Vector3.Normalize(p1.m_position.Flatten() - p2.m_position.Flatten());
			var d2 = Vector3.Normalize(p3.m_position.Flatten() - p2.m_position.Flatten());

			var angle = Vector3.Angle(d1, d2);
			var angleDirection = Vector3.Normalize(d1 + d2);

			measurements.Add(new AngleMeasurement(angle, p2.m_position, angleDirection,
				MeasurementFlags.Primary | MeasurementFlags.Blueprint));

		}

		/// <summary>
		/// Calculate the distance between all the proposed nodes
		/// </summary>
		/// <param name="netTool"></param>
		/// <param name="measurements"></param>
		private static void CalculateNodePositionDistance(NetToolProxy netTool, ICollection<Measurement> measurements)
		{

			if (netTool.NodePositions.m_size <= 1)
				return;

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

			measurements.Add(d);

		}

		private static readonly ushort[] _segments = new ushort[16];

		private static void CalculateNearbySegments(NetToolProxy netTool, ICollection<Measurement> measurements)
		{

			if (netTool.ControlPointsCount == 0)
				return;

			if (netTool.NodePositions.m_size <= 1)
				return;


			var lastNode = netTool.NodePositions[netTool.NodePositions.m_size - 1];

			int count;

			NetManager.instance.GetClosestSegments(lastNode.m_position, _segments, out count);

			if (count == 0)
				return;

			/*//List<ushort> sourceNodeConnectedSegmentIds = null;

			if (netTool.ControlPoints[0].m_node > 0) {

				sourceNodeConnectedSegmentIds =
					NetNodeUtility.GetNodeSegmentIds(NetManager.instance.m_nodes.m_buffer[netTool.ControlPoints[0].m_node]);

			}*/
			var p1 = lastNode.m_position;

			var minDist = float.MaxValue;
			var p = Vector3.zero;
			var found = false;

			for (var i = 0; i < count; i++) {

				/*// Skip segments attached to the node we are building from
				if (sourceNodeConnectedSegmentIds != null && sourceNodeConnectedSegmentIds.Contains(_segments[i]))
					continue;*/

				if (netTool.ControlPoints[0].m_segment > 0 && _segments[i] == netTool.ControlPoints[0].m_segment)
					continue;

				var s = NetManager.instance.m_segments.m_buffer[_segments[i]];

				if (s.Info.m_class.m_service != netTool.NetInfo.m_class.m_service)
					continue;

				var p2 = s.GetClosestPosition(p1);

				var dist = Vector3.Distance(p1, p2);

				if (dist < minDist) {

					minDist = dist;
					p = p2;
					found = true;

				}

			}

			if (found && minDist > Settings.MinimumDistanceMeasure) {

				measurements.Add(new DistanceMeasurement(Vector3.Distance(p1, p), Vector3Extensions.Average(p1, p), true, p1, p,
					MeasurementFlags.Secondary));

			}

		}


	}
}
