using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PrecisionEngineering.Data.Calculations
{
	class Segment
	{

		/// <summary>
		/// Calculate the angles between branch and the segment it branches from
		/// </summary>
		/// <param name="netTool"></param>
		/// <param name="measurements">Collection to populate with measurements</param>
		public static void CalculateSegmentBranchAngles(NetToolProxy netTool, ICollection<Measurement> measurements)
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

			measurements.Add(new AngleMeasurement(angleSize, sourceNode.m_position, angleDirection,
				angleSize > otherAngleSize ? MeasurementFlags.Secondary : MeasurementFlags.Primary));

			measurements.Add(new AngleMeasurement(otherAngleSize, sourceNode.m_position, otherAngleDirection,
				angleSize > otherAngleSize ? MeasurementFlags.Primary : MeasurementFlags.Secondary));

		}

	}
}
