using UnityEngine;

namespace PrecisionEngineering.Data
{
	class DistanceMeasurement : Measurement
	{

		public float Length { get; private set; }

		public bool IsStraight { get; private set; }

		public Vector3 StartPosition { get; private set; }

		public Vector3 EndPosition { get; private set; }

		public float RelativeHeight { get { return EndPosition.y - StartPosition.y; } }

		public DistanceMeasurement(float length, Vector3 position, bool isStraight, Vector3 startPosition, Vector3 endPosition, MeasurementFlags flags)
			: base(position, flags)
		{
			Length = length;
			IsStraight = isStraight;
			StartPosition = startPosition;
			EndPosition = endPosition;
		}

		public override string ToString()
		{
			return string.Format("Distance: {0}m, @{1}, IsStraight: {2}", Length, Position, IsStraight);
		}

	}
}