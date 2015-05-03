using System;
using ColossalFramework;
using UnityEngine;

namespace PrecisionEngineering.Data
{

	[Flags]
	enum MeasurementFlags : int
	{

		None = 0,
		Primary = 0x1,
		Secondary = 0x2,
		HideOverlay = 0x4

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