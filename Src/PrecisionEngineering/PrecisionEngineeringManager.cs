using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		void Start()
		{

			Debug.Log("Manager Start");

			_netTool = Object.FindObjectOfType<NetTool>();
			_netToolProxy = new NetToolProxy(_netTool);

			_calculator = new PrecisionCalculator();

			Debug.Log(string.Format("Net Tool: {0}", _netTool));

			_ui = new PrecisionUI();

			if (Debug.Enabled)
				_ui.CreateDebugUI(_netToolProxy, _calculator);

		}

		protected override void SimulationStepImpl(int subStep)
		{

			base.SimulationStepImpl(subStep);

			_calculator.Update(_netToolProxy);

		}

		protected override void EndOverlayImpl(RenderManager.CameraInfo cameraInfo)
		{

			base.EndOverlayImpl(cameraInfo);

			_ui.ReleaseAll();

			for (var i = 0; i < _calculator.Measurements.Count; i++) {

				var m = _calculator.Measurements[i];

				if (m is AngleMeasurement) {

					var am = m as AngleMeasurement;

					Rendering.AngleRenderer.Render(cameraInfo, am);

					var label = _ui.GetAngleLabel();
					label.SetAngle(am.AngleSize);
					label.SetWorldPosition(cameraInfo, Rendering.AngleRenderer.GetLabelWorldPosition(am));

					continue;

				}

				Debug.LogError("Measurement has no renderer: " + m.ToString());

			}

		}

	}

}
