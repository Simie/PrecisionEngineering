using PrecisionEngineering.Data;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Rendering
{
    internal static class AngleRenderer
    {
        public static float GetAngleDistance(MeasurementFlags flags)
        {
            if ((flags & MeasurementFlags.Blueprint) != 0)
            {
                return 10f;
            }

            return 15f;
        }

        public static Color GetAngleColor(MeasurementFlags flags)
        {
            if ((flags & MeasurementFlags.Blueprint) != 0)
            {
                return Settings.BlueprintColor;
            }

            if ((flags & MeasurementFlags.Secondary) != 0)
            {
                return Settings.SecondaryColor;
            }

            return Settings.PrimaryColor;
        }

        public static Vector3 GetLabelWorldPosition(AngleMeasurement angle)
        {
            return angle.Position + angle.AngleNormal*GetAngleDistance(angle.Flags);
        }

        public static void Render(RenderManager.CameraInfo cameraInfo, AngleMeasurement angle)
        {
            if (angle.HideOverlay)
            {
                return;
            }

            var renderManager = RenderManager.instance;

            var centreAngle = Vector3.Angle(Vector3.right, angle.AngleNormal);

            if (Vector3.Cross(Vector3.right, angle.AngleNormal).y > 0f)
            {
                centreAngle = -centreAngle;
            }

            var arcs = BezierUtil.CreateArc(angle.Position, GetAngleDistance(angle.Flags),
                centreAngle - angle.AngleSize*.5f,
                centreAngle + angle.AngleSize*.5f);

            for (var i = 0; i < arcs.Count; i++)
            {
                var isFirst = i == 0;
                var isLast = i == arcs.Count - 1;

                renderManager.OverlayEffect.DrawBezier(cameraInfo, GetAngleColor(angle.Flags), arcs[i], .7f, 0f, 0f,
                    angle.Position.y - 20f,
                    angle.Position.y + 20f, false, true);
            }
        }
    }
}
