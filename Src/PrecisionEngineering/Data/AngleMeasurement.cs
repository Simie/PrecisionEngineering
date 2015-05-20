using UnityEngine;

namespace PrecisionEngineering.Data
{
	class AngleMeasurement : Measurement
	{

		public float AngleSize { get; private set; }

		public Vector3 AngleNormal { get; private set; }

		public AngleMeasurement(float size, Vector3 position, Vector3 normal, MeasurementFlags flags)
			: base(position, flags)
		{
			AngleSize = size;
			AngleNormal = normal;
		}

		public override string ToString()
		{
			return
				string.Format("Angle: {0}deg, @{1}, facing: {2}\n\t{3}", AngleSize, Position, AngleNormal, Flags);
		}

	}
}