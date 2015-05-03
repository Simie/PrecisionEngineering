using ColossalFramework.UI;
using UnityEngine;

namespace PrecisionEngineering.UI
{
	class AngleLabel : UILabel
	{

		private float _angle;

		public void SetAngle(float angle)
		{

			var a = angle.RoundToNearest(0.1f);

			if (a == _angle)
				return;

			text = string.Format("<color=#87d3ff>{0:#.0}</color>°", a);
			_angle = angle;

		}

		public void SetWorldPosition(RenderManager.CameraInfo camera, Vector3 worldPos)
		{

			var uiView = GetUIView();
			var vector2 = !(ToolBase.fullscreenContainer != null) ? uiView.GetScreenResolution() : ToolBase.fullscreenContainer.size;
			var vector3_1 = Camera.main.WorldToScreenPoint(worldPos) / uiView.inputScale;

			//var vector3_2 = pivot.UpperLeftToTransform(size, arbitraryPivotOffset);
			var vector3_3 = uiView.ScreenPointToGUI(vector3_1) - new Vector2(size.x*0.5f, size.y*0.5f);// + new Vector2(vector3_2.x, vector3_2.y);

			relativePosition = vector3_3;

		}

		public override void Start()
		{

			base.Start();

			_angle = float.MaxValue;

			backgroundSprite = "CursorInfoBack";
			autoSize = true;
			padding = new RectOffset(5,5,5,5);
			textScale = 0.8f;
			textAlignment = UIHorizontalAlignment.Center;
			verticalAlignment = UIVerticalAlignment.Middle;
			zOrder = 16;
			
			pivot = UIPivotPoint.MiddleCenter;

			color = new Color32(255,255,255,180);
			processMarkup = true;

			//<color #87d3ff>Construction cost: 520</color>

		}

	}
}
