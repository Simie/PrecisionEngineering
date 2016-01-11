using ColossalFramework.Math;
using PrecisionEngineering.Data;

namespace PrecisionEngineering.Rendering
{
    internal static class GuideLineRenderer
    {
        public static void Render(RenderManager.CameraInfo cameraInfo, GuideLine guideLine)
        {
            var renderManager = RenderManager.instance;

            var minHeight = -1f;
            var maxHeight = 1280f;

            var direction = guideLine.Origin.Flatten().DirectionTo(guideLine.Intersect.Flatten());

            var line = new Segment3(guideLine.Origin, guideLine.Origin + direction*100000f);

            renderManager.OverlayEffect.DrawSegment(cameraInfo, Settings.SecondaryColor,
                line, guideLine.Width, 0,
                minHeight,
                maxHeight, true, true);

            renderManager.OverlayEffect.DrawSegment(cameraInfo, Settings.SecondaryColor,
                line, 0.01f, 8f,
                minHeight,
                maxHeight, true, true);
        }
    }
}
