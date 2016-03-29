using UnityEngine;

namespace PrecisionEngineering
{
    internal static class Settings
    {
        public const float MinimumDistanceMeasure = 0.9f;

        public const float SnapAngle = 5;

        public const float GuideLinesVisibilityDistance = 80f;
        public const float GuideLinesSnapDistance = 5f;

        /// <summary>
        /// Number of segments to query for guide-lines. More = better result, slower performance
        /// </summary>
        public const int GuideLineQueryCount = 512;

        public const int MaxGuideLineQueryDistance = 1024;
        public const int MaxGuideLineQueryDistanceSqr = MaxGuideLineQueryDistance*MaxGuideLineQueryDistance;

        public static Color BlueprintColor = new Color(1, 1, 1, 1);
        public static Color PrimaryColor = Color.green;
        public static Color SecondaryColor = Color.yellow;
    }
}
