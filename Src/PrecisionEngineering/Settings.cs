using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PrecisionEngineering
{
	static class Settings
	{

		public const float MinimumDistanceMeasure = 3;

		public const float SnapAngle = 5;

		public static Color BlueprintColor = new Color(1,1,1,1);
		public static Color PrimaryColor = Color.green;
		public static Color SecondaryColor = Color.yellow;

		public const float GuideLinesVisibilityDistance = 80f;
		public const float GuideLinesSnapDistance = 5f;

		/// <summary>
		/// Number of segments to query for guide-lines. More = better result, slower performance
		/// </summary>
		public const int GuideLineQueryCount = 512;

		public const int MaxGuideLineQueryDistance = 1024;
		public const int MaxGuideLineQueryDistanceSqr = MaxGuideLineQueryDistance * MaxGuideLineQueryDistance;

	}
}
