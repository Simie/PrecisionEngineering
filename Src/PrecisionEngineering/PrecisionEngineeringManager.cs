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

	internal class PrecisionEngineeringManager : SimulationManagerBase<PrecisionEngineeringManager, MonoBehaviour>,
		ISimulationManager, IRenderableManager
	{

		private static bool _hasRegistered;

		public static void OnLevelLoaded()
		{

			if (_hasRegistered)
				return;

			Debug.Log("Registering Manager");

			SimulationManager.RegisterManager(instance);
			_hasRegistered = true;

		}

		protected NetToolProxy NetToolProxy
		{
			get
			{

				if (_netToolProxy == null || !_netToolProxy.IsValid) {

					_netTool = FindObjectOfType<NetTool>();
					_netToolProxy = new NetToolProxy(_netTool);

					_ui.NetToolProxy = _netToolProxy;

				}

				return _netToolProxy;
				
			}
		}

		private NetTool _netTool;
		private NetToolProxy _netToolProxy;

		private PrecisionCalculator _calculator;

		private readonly PrecisionUI _ui = new PrecisionUI();
		private bool _secondaryDetailEnabled;

		private bool _isLoaded = false;

		private void Load()
		{

			if (_isLoaded)
				return;

			_isLoaded = true;

			Debug.Log("Manager Load");

			Settings.BlueprintColor = NetToolProxy.ToolController.m_validColor;

			_calculator = new PrecisionCalculator();
			_ui.Calculator = _calculator;

		}

		protected override void SimulationStepImpl(int subStep)
		{

			if (!_isLoaded)
				Load();

			base.SimulationStepImpl(subStep);

			SnapController.EnableSnapping = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			
			_calculator.Update(NetToolProxy);

		}

		protected override void EndOverlayImpl(RenderManager.CameraInfo cameraInfo)
		{

			base.EndOverlayImpl(cameraInfo);
			
			if (!_isLoaded)
				return;

			_ui.ReleaseAll();

			// Toggle with shift
			/*_secondaryDetailEnabled = (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
				? !_secondaryDetailEnabled
				: _secondaryDetailEnabled;*/

			// Activate with shift
			_secondaryDetailEnabled = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

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

				    if (Mathf.Approximately(dm.Length, 0f))
				        continue;

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