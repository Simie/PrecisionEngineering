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

		/// <summary>
		/// Managers should only be registered once, and then they persist over loads
		/// </summary>
		private static bool _hasRegistered;

		public static void OnLevelLoaded()
		{

			if (!_hasRegistered) {

				Debug.Log("Registering Manager");

				SimulationManager.RegisterManager(instance);
				_hasRegistered = true;

			}

			instance.Load();

		}

		public static void OnLevelUnloaded()
		{
			instance.Unload();
		}

		public bool IsEnabled
		{
			get { return _isLoaded && _netToolProxy != null && _netToolProxy.IsValid; }
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

			_calculator = new PrecisionCalculator();
			_ui.Calculator = _calculator;

		}

		private void Unload()
		{

			Debug.Log("Manager Unload");

			_netToolProxy = null;
			_netTool = null;

			_isLoaded = false;

		}

		private void Update()
		{

			if (_isLoaded && !IsEnabled) {

				Debug.Log("Loading NetTool");

				_netTool = FindObjectOfType<NetTool>();

				Debug.Log("NetTool: " + _netTool);

				if (_netTool != null) {

					_netToolProxy = new NetToolProxy(_netTool);
					_ui.NetToolProxy = _netToolProxy;
					Settings.BlueprintColor = _netToolProxy.ToolController.m_validColor;

				}

			}

		}

		protected override void SimulationStepImpl(int subStep)
		{

			if (!IsEnabled)
				return;

			base.SimulationStepImpl(subStep);

			SnapController.EnableSnapping = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

			_calculator.Update(_netToolProxy);

		}

		protected override void EndOverlayImpl(RenderManager.CameraInfo cameraInfo)
		{

			base.EndOverlayImpl(cameraInfo);

			if (!IsEnabled)
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

					if (am.AngleSize < 1f)
						continue;

					Rendering.AngleRenderer.Render(cameraInfo, am);

					var label = _ui.GetMeasurementLabel();
					label.SetValue(string.Format("{0:#.0}{1}", am.AngleSize.RoundToNearest(0.1f), "°"));
					label.SetWorldPosition(cameraInfo, Rendering.AngleRenderer.GetLabelWorldPosition(am));

					continue;

				}

				if (m is DistanceMeasurement) {


					var dm = m as DistanceMeasurement;

					if (dm.Length < 7f)
						continue;

					Rendering.DistanceRenderer.Render(cameraInfo, dm);

					var label = _ui.GetMeasurementLabel();

					var dist = string.Format("{0:#}{1}", (dm.Length/8f).RoundToNearest(1), "u");

					if (_secondaryDetailEnabled) {

						dist += string.Format(" ({0}{1})", (int)(dm.Length).RoundToNearest(1), "m");

						var heightdiff = (int)(dm.RelativeHeight).RoundToNearest(1);

						if (Mathf.Abs(heightdiff) > 0) {
							dist += string.Format("\n(Slope: {0}{1})", heightdiff, "m");
						}

					}

					label.SetValue(dist);
					label.SetWorldPosition(cameraInfo, Rendering.DistanceRenderer.GetLabelWorldPosition(dm));

					continue;

				}

				Debug.LogError("Measurement has no renderer: " + m.ToString());

			}

		}

	}

}