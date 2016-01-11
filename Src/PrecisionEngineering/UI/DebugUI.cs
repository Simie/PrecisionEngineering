using System.Text;
using ColossalFramework.UI;
using PrecisionEngineering.Data;
using PrecisionEngineering.Detour;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.UI
{
    internal class DebugUI : UIPanel
    {
        private bool _hasStarted;

        private UILabel _label;
        public PrecisionCalculator Calculator;

        public NetToolProxy NetTool;

        public override void Start()
        {
            base.Start();

            backgroundSprite = "GenericPanel";
            opacity = 0.7f;
            width = 550;
            height = 700;
            canFocus = false;
            relativePosition = new Vector3(10, 80f);

            _label = AddUIComponent<UILabel>();
            _label.relativePosition = new Vector3(0, 0);
            _label.maximumSize = new Vector2(width - 20, height - 20);
            _label.padding = new RectOffset(10, 10, 10, 10);

            _hasStarted = true;
        }

        public void DoUpdate()
        {
            if (!_hasStarted)
            {
                return;
            }

            if (NetTool == null)
            {
                _label.text = "NetTool not set";
                return;
            }

            if (Calculator == null)
            {
                _label.text = "Calculator not set";
                return;
            }

            var txt = new StringBuilder();

            /*txt.AppendLine(string.Format("ToolManager.instance.m_properties.HasInputFocus: {0}",
				ToolManager.instance.m_properties.HasInputFocus));

			txt.AppendLine(string.Format("LCtrl: {0}, RCtrl: {1}, LShift: {2}, RShift: {3},\n LAlt: {4}, RAlt: {5}, AltGR: {6}",
				Input.GetKey(KeyCode.LeftControl), Input.GetKey(KeyCode.RightControl), Input.GetKey(KeyCode.LeftShift),
				Input.GetKey(KeyCode.RightShift), Input.GetKey(KeyCode.LeftAlt), Input.GetKey(KeyCode.RightAlt),
				Input.GetKey(KeyCode.AltGr)));*/

            txt.AppendLine(string.Format("SnapController:\n{0}", SnapController.DebugPrint));
            SnapController.DebugPrint = "";

            txt.AppendLine(string.Format("SnapController.EnableLengthSnapping: {0}", SnapController.EnableLengthSnapping));
            txt.AppendLine(string.Format("NetToolProxy.IsEnabled: {0}", NetTool.IsEnabled));

            txt.Append(string.Format("Control Point Count: {0}", NetTool.ControlPointsCount));
            txt.AppendLine(string.Format(", Node Count: {0}", NetTool.NodePositions.m_size));

            txt.AppendLine("Control Points: ");

            for (var i = 0; i < NetTool.ControlPointsCount + 1; i++)
            {
                var pt = NetTool.ControlPoints[i];
                txt.AppendLine(StringUtil.ToString(pt));
            }

            txt.AppendLine();

            txt.AppendLine("Nodes: ");

            for (var i = 0; i < NetTool.NodePositions.m_size; i++)
            {
                var pt = NetTool.NodePositions[i];
                txt.AppendLine(StringUtil.ToString(pt));
            }

            txt.AppendLine("-----------------");
            txt.AppendLine("Measurements: ");

            if (Calculator.Measurements.Count == 0)
            {
                txt.AppendLine("No Measurements Available");
            }

            for (var i = 0; i < Calculator.Measurements.Count; i++)
            {
                txt.AppendLine(Calculator.Measurements[i].ToString());
            }
            txt.AppendLine("-----------------");
            txt.Append("Guide Lines: ");
            txt.AppendLine(string.Format("({0})", SnapController.GuideLines.Count));

            if (SnapController.GuideLines.Count == 0)
            {
                txt.AppendLine("No GuideLines Available");
            }

            for (var i = 0; i < Mathf.Min(10, SnapController.GuideLines.Count); i++)
            {
                txt.AppendLine(SnapController.GuideLines[i].ToString());
            }

            txt.AppendLine();
            txt.AppendLine(Calculator.DebugState);

            _label.text = txt.ToString();
        }
    }
}
