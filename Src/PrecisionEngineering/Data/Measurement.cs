using UnityEngine;

namespace PrecisionEngineering.Data
{

	enum MeasurementDetail
	{

		Primary,
		Secondary

	}

	abstract class Measurement
	{

		public MeasurementDetail Detail { get; private set; }
		public Vector3 Position { get; private set; }

		protected Measurement(Vector3 position, MeasurementDetail detail)
		{
			Position = position;
			Detail = detail;
		}

	}

	class AngleMeasurement : Measurement
	{

		public float AngleSize { get; private set; }

		public Vector3 AngleNormal { get; private set; }

		public AngleMeasurement(float size, Vector3 position, Vector3 normal, MeasurementDetail detail)
			: base(position, detail)
		{
			AngleSize = size;
			AngleNormal = normal;
		}

		public override string ToString()
		{
			return
				string.Format("Angle: {0}deg, @{1}, facing: {2}", AngleSize, Position, AngleNormal);
		}

	}

	class DistanceMeasurement : Measurement
	{

		public float Length { get; private set; }

		public DistanceMeasurement(float length, Vector3 position, MeasurementDetail detail)
			: base(position, detail)
		{
			Length = length;
		}

		public override string ToString()
		{
			return string.Format("Distance: {0}m, @{1}", Length, Position);
		}

	}

}