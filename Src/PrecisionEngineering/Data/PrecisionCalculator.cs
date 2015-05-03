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
			Node.CalculateBranchAngles(netTool, _measurements);

			CalculateDistance(netTool, _measurements);
			CalculateNearbySegments(netTool, _measurements);

		}

		private static void CalculateDistance(NetToolProxy netTool, ICollection<Measurement> measurements)
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
