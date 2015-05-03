using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace PrecisionEngineering.UI
{
	class DebugUI : UIPanel
	{

		public NetToolProxy NetTool;
		public PrecisionCalculator Calculator;

		private UILabel _label;

		public override void Start()
		{

			base.Start();

			this.backgroundSprite = "GenericPanel";

			this.width = 600;
			this.height = 400;

			this.relativePosition = new Vector3(10, 80f);

			_label = this.AddUIComponent<UILabel>();
			_label.relativePosition = new Vector3(0, 0);
			_label.maximumSize = new Vector2(width - 20, height - 20);
			_label.padding = new RectOffset(10, 10, 10, 10);

		}

		public override void LateUpdate()
		{

			base.LateUpdate();

			var txt = new StringBuilder();

			txt.Append(string.Format("Control Point Count: {0}", NetTool.ControlPointsCount));
			txt.AppendLine(string.Format(", Node Count: {0}", NetTool.NodePositions.m_size));

			txt.AppendLine("Control Points: ");

			for (var i = 0; i < NetTool.ControlPointsCount; i++) {

				var pt = NetTool.ControlPoints[i];

				txt.AppendLine(string.Format("\tPosition: {0}, Direction: {1}", pt.m_position, pt.m_direction));
				txt.AppendLine(string.Format("\t\t, Node: {0}, Segment: {1}", pt.m_node, pt.m_segment));

			}

			txt.AppendLine();
			txt.AppendLine("Nodes: ");

			for (var i = 0; i < NetTool.NodePositions.m_size; i++) {

				var pt = NetTool.NodePositions[i];
				txt.AppendLine(string.Format("\tPosition: {0}, Direction: {1}", pt.m_position,
					pt.m_direction));

			}

			txt.AppendLine();

			if (Calculator.Measurements.Count == 0)
				txt.AppendLine("No Measurements Available");

			for (var i = 0; i < Calculator.Measurements.Count; i++) {

				txt.AppendLine(Calculator.Measurements[i].ToString());

			}

			txt.AppendLine();
			txt.AppendLine(Calculator.DebugState);

			_label.text = txt.ToString();

		}


	}
}
