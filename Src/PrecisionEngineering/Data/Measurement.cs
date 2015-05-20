using System;
using ColossalFramework;
using UnityEngine;

namespace PrecisionEngineering.Data
{

	[Flags]
	enum MeasurementFlags : int
	{

		None = 0,
		Primary = 1 << 0,
		Secondary = 1 << 1,

		/// <summary>
		/// Hide any rendering overlays for this measurement
		/// </summary>
		HideOverlay = 1 << 2,

		/// <summary>
		/// This measurement is part of a blueprint operation (ie placing control points of a curve)
		/// </summary>
		Blueprint = 1 << 3,

		/// <summary>
		/// This measurement is used as a snapping guide
		/// </summary>
		Guide = 1 << 4,

		/// <summary>
		/// Indicate that a PositionMeasurement is a height measurement
		/// </summary>
		Height = 1 << 5,

	}

	abstract class Measurement
	{

		public MeasurementFlags Flags { get; private set; }
		public Vector3 Position { get; private set; }

		public bool HideOverlay { get { return Flags.IsFlagSet(MeasurementFlags.HideOverlay); } }

		protected Measurement(Vector3 position, MeasurementFlags flags)
		{
			Position = position;
			Flags = flags;
		}

	}
}