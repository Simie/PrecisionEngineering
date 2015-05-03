using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace PrecisionEngineering.UI
{
	class MeasurementLabel : UILabel
	{

		public void SetValue(float value, string suffix)
		{

			var a = value.RoundToNearest(0.1f);
			text = string.Format("<color=#87d3ff>{0:#.0}</color>{1}", a, suffix);

		}

		public void SetWorldPosition(RenderManager.CameraInfo camera, Vector3 worldPos)
		{

			var uiView = GetUIView();

			var vector3_1 = Camera.main.WorldToScreenPoint(worldPos) / uiView.inputScale;
			var vector3_3 = uiView.ScreenPointToGUI(vector3_1) - new Vector2(size.x * 0.5f, size.y * 0.5f);// + new Vector2(vector3_2.x, vector3_2.y);

			relativePosition = vector3_3;

		}

		public override void Start()
		{

			base.Start();

			backgroundSprite = "CursorInfoBack";
			autoSize = true;
			padding = new RectOffset(5, 5, 5, 5);
			textScale = 0.8f;
			textAlignment = UIHorizontalAlignment.Center;
			verticalAlignment = UIVerticalAlignment.Middle;
			zOrder = 16;

			pivot = UIPivotPoint.MiddleCenter;

			color = new Color32(255, 255, 255, 180);
			processMarkup = true;

			//<color #87d3ff>Construction cost: 520</color>

		}

	}
}
