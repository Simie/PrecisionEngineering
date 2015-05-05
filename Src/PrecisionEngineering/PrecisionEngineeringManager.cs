using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using PrecisionEngineering.Data;
using PrecisionEngineering.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PrecisionEngineering
{

	class PrecisionEngineeringManager : SimulationManagerBase<PrecisionEngineeringManager, MonoBehaviour>, ISimulationManager, IRenderableManager
	{

		private NetTool _netTool;
		private NetToolProxy _netToolProxy;

		private PrecisionCalculator _calculator;

		private PrecisionUI _ui;
		private bool _secondaryDetailEnabled;

		void Start()
		{

			Debug.Log("Manager Start");

			_netTool = FindObjectOfType<NetTool>();
			_netToolProxy = new NetToolProxy(_netTool);

			Settings.BlueprintColor = _netToolProxy.ToolController.m_validColor;

			_calculator = new PrecisionCalculator();

			Debug.Log(string.Format("Net Tool: {0}", _netTool));

			_ui = new PrecisionUI();

			if (Debug.Enabled)
				_ui.CreateDebugUI(_netToolProxy, _calculator);

		}

		protected override void SimulationStepImpl(int subStep)
		{

			base.SimulationStepImpl(subStep);

			SnapController.EnableSnapping = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			_calculator.Update(_netToolProxy);

		}

		protected override void EndOverlayImpl(RenderManager.CameraInfo cameraInfo)
		{

			base.EndOverlayImpl(cameraInfo);

			_ui.ReleaseAll();

			_secondaryDetailEnabled = (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
				? !_secondaryDetailEnabled
				: _secondaryDetailEnabled;

			for (var i = 0; i < _calculator.Measurements.Count; i++) {

				var m = _calculator.Measurements[i];

				if (m.Flags == MeasurementFlags.Secondary && !_secondaryDetailEnabled)
					continue;

				if (m is AngleMeasurement) {

					var am = m as AngleMeasurement;

					Rendering.AngleRenderer.Render(cameraInfo, am);

					var label = _ui.GetMeasurementLabel();
					label.SetValue(string.Format("{0:#.0}{1}", am.AngleSize.RoundToNearest(0.1f), "°"));
					label.SetWorldPosition(cameraInfo, Rendering.AngleRenderer.GetLabelWorldPosition(am));

					continue;

				}

				if (m is DistanceMeasurement) {


					var dm = m as DistanceMeasurement;

					Rendering.DistanceRenderer.Render(cameraInfo, dm);

					var label = _ui.GetMeasurementLabel();
					label.SetValue(string.Format("{0:#}{1}", dm.Length.RoundToNearest(1), "m"));
					label.SetWorldPosition(cameraInfo, Rendering.DistanceRenderer.GetLabelWorldPosition(dm));

					continue;

				}

				Debug.LogError("Measurement has no renderer: " + m.ToString());

			}

		}

	}

}
