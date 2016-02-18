using System;
using ColossalFramework.UI;
using UnityEngine;

namespace PrecisionEngineering.UI
{
    internal class MeasurementLabel : UILabel
    {
        public void SetValue(string v)
        {
            text = string.Format("<color=#87d3ff>{0}</color>", v);
        }

        public void SetWorldPosition(RenderManager.CameraInfo camera, Vector3 worldPos)
        {
            var uiView = GetUIView();

            var vector3_1 = Camera.main.WorldToScreenPoint(worldPos)/uiView.inputScale;
            var vector3_3 = uiView.ScreenPointToGUI(vector3_1) - new Vector2(size.x*0.5f, size.y*0.5f);
                // + new Vector2(vector3_2.x, vector3_2.y);

            relativePosition = vector3_3;
            textScale = GetFontScale();
        }

        public override void Start()
        {
            base.Start();

            backgroundSprite = "CursorInfoBack";
            autoSize = true;
            padding = new RectOffset(5, 5, 5, 5);
            textScale = GetFontScale();
            textAlignment = UIHorizontalAlignment.Center;
            verticalAlignment = UIVerticalAlignment.Middle;
            zOrder = 100;

            pivot = UIPivotPoint.MiddleCenter;

            color = new Color32(255, 255, 255, 190);
            processMarkup = true;

            isInteractive = false;

            //<color #87d3ff>Construction cost: 520</color>
        }

        private static float GetFontScale()
        {
            var size = ModSettings.FontSize;
            switch (size)
            {
                case 0:
                    return 0.65f;
                case 1:
                    return 0.8f;
                case 2:
                    return 1.1f;
            }

            throw new IndexOutOfRangeException();
        }
    }
}
