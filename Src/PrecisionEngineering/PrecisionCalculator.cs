using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using UnityEngine;

namespace PrecisionEngineering
{

	enum MeasurementDetail
	{

		Common,
		Extra

	}

	abstract class Measurement
	{

		public MeasurementDetail Detail { get; private set; }

		protected Measurement(MeasurementDetail detail)
		{
			Detail = detail;
		}

	}

	class AngleMeasurement : Measurement
	{

		public float AngleSize { get; private set; }

		public Vector3 AnglePosition { get; private set; }

		public Vector3 AngleNormal { get; private set; }

		public AngleMeasurement(float size, Vector3 position, Vector3 normal, MeasurementDetail detail) : base(detail)
		{
			AngleSize = size;
			AnglePosition = position;
			AngleNormal = normal;
		}

		public override string ToString()
		{
			return
				string.Format("Angle: {0}deg, @{1}, facing: {2}", AngleSize, AnglePosition, AngleNormal);
		}

	}

	class DistanceMeasurement : Measurement
	{


		public DistanceMeasurement(MeasurementDetail detail)
			: base(detail)
		{
			
		}

	}

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

			if(netTool.ControlPointsCount > 0 && netTool.ControlPoints[0].m_segment > 0)
				CalculateSegmentBranchAngles(netTool);

			if (netTool.ControlPointsCount > 0 && netTool.ControlPoints[0].m_node > 0)
				CalculateNodeBranchAngles(netTool);

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
				angleSize > otherAngleSize ? MeasurementDetail.Extra : MeasurementDetail.Common));

			_measurements.Add(new AngleMeasurement(otherAngleSize, sourceNode.m_position, otherAngleDirection,
				angleSize > otherAngleSize ? MeasurementDetail.Common : MeasurementDetail.Extra));

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

			_measurements.Add(new AngleMeasurement(angleSize, sourceNode.m_position, angleDirection, MeasurementDetail.Common));

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
