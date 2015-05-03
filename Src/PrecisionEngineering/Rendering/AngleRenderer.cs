using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Rendering
{
	static class AngleRenderer
	{

		public const float AngleSize = 35f;

		public static Vector3 GetLabelWorldPosition(AngleMeasurement angle)
		{

			return angle.AnglePosition + angle.AngleNormal*AngleSize;

		}

		public static void Render(RenderManager.CameraInfo cameraInfo, AngleMeasurement angle)
		{

			var renderManager = RenderManager.instance;


			var centreAngle = Vector3.Angle(Vector3.right, angle.AngleNormal);

			if (Vector3.Cross(Vector3.right, angle.AngleNormal).y > 0f)
				centreAngle = -centreAngle;


			var arcs = BezierUtil.CreateArc(angle.AnglePosition, AngleSize,
				centreAngle - angle.AngleSize*.5f,
				centreAngle + angle.AngleSize*.5f);

			for (var i = 0; i < arcs.Count; i++) {

				var isFirst = i == 0;
				var isLast = i == arcs.Count - 1;

				renderManager.OverlayEffect.DrawBezier(cameraInfo,
					angle.Detail == MeasurementDetail.Common ? Color.green : Color.yellow, arcs[i], 3f, 0f, 0f,
					angle.AnglePosition.y - 20f,
					angle.AnglePosition.y + 20f, true, true);

			}

		}

	}
}
