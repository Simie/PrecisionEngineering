using System;
using ColossalFramework;
using UnityEngine;

namespace PrecisionEngineering.Data
{
    [Flags]
    internal enum MeasurementFlags
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

        /// <summary>
        /// Indicate that this measurement is used for snapping and should be visable when
        /// snapping is enabled.
        /// </summary>
        Snap = 1 << 6
    }

    internal abstract class Measurement
    {
        protected Measurement(Vector3 position, MeasurementFlags flags)
        {
            Position = position;
            Flags = flags;
        }

        public MeasurementFlags Flags { get; }

        /// <summary>
        /// Where in world-space the measurement is located.
        /// </summary>
        public Vector3 Position { get; private set; }

        public bool HideOverlay
        {
            get { return Flags.IsFlagSet(MeasurementFlags.HideOverlay); }
        }
    }
}
