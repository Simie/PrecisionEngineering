using System;
using ColossalFramework.UI;
using PrecisionEngineering.Data;
using PrecisionEngineering.Detour;
using PrecisionEngineering.Rendering;
using PrecisionEngineering.UI;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering
{
    internal class Manager : SimulationManagerBase<Manager, MonoBehaviour>,
        ISimulationManager, IRenderableManager
    {
        /// <summary>
        /// Managers should only be registered once, and then they persist over loads
        /// </summary>
        private static bool _hasRegistered;
        private bool _isLoaded;

        private readonly PrecisionCalculator _calculator = new PrecisionCalculator();
        private readonly PrecisionUI _ui = new PrecisionUI();

        private bool _advancedSnappingEnabled;
        private bool _secondaryDetailEnabled;

        private NetToolProxy _netToolProxy;

        public bool IsEnabled
        {
            get { return _netToolProxy != null && _netToolProxy.IsValid; }
        }

        public static void OnLevelLoaded()
        {
            if (!_hasRegistered)
            {
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

        private void Load()
        {
            if (_isLoaded)
            {
                return;
            }

            _isLoaded = true;

            Debug.Log("Manager Load");

            _ui.Calculator = _calculator;
        }

        private void Unload()
        {
            Debug.Log("Manager Unload");

            _netToolProxy = null;
            _isLoaded = false;

            _ui.ReleaseAll();
        }

        private void Update()
        {
            // Search for NetTool if not already loaded.
            // Performing this in the default MonoBehaviour so that we will
            // correctly catch any modded NetTools that replace the default.

            if (_isLoaded && !IsEnabled)
            {
                Debug.Log("Loading NetTool");

                _netToolProxy = NetToolLocator.Locate();

                if (_netToolProxy != null)
                {
                    Settings.BlueprintColor = _netToolProxy.ToolController.m_validColor;
                    _ui.NetToolProxy = _netToolProxy;
                }
            }
        }

        protected override void SimulationStepImpl(int subStep)
        {
            if (!IsEnabled)
            {
                //Debug.LogWarning("[SimulationStepImpl] !IsEnabled");
                return;
            }

            base.SimulationStepImpl(subStep);

            // Toggle with shift
            /*_secondaryDetailEnabled = (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
				? !_secondaryDetailEnabled
				: _secondaryDetailEnabled;*/

            // Activate with shift
            _secondaryDetailEnabled = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            _advancedSnappingEnabled = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            SnapController.EnableSnapping = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            SnapController.EnableAdvancedSnapping = _advancedSnappingEnabled;
            SnapController.EnableLengthSnapping = !SnapController.EnableAdvancedSnapping;

            if (!SnapController.EnableAdvancedSnapping)
            {
                SnapController.SnappedGuideLine = null;
                SnapController.GuideLines.Clear();
            }

            _calculator.Update(_netToolProxy);
        }

        protected override void EndOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            base.EndOverlayImpl(cameraInfo);

            if (!IsEnabled)
            {
                //Debug.LogWarning("[EndOverlayImpl] !IsEnabled");
                return;
            }

            _ui.ReleaseAll();

            for (var i = 0; i < _calculator.Measurements.Count; i++)
            {
                HandleMeasurement(cameraInfo, _calculator.Measurements[i]);
            }

            lock (SnapController.GuideLineLock)
            {
                if (SnapController.SnappedGuideLine.HasValue)
                {
                    GuideLineRenderer.Render(cameraInfo, SnapController.SnappedGuideLine.Value);
                }
            }

            try
            {
                _ui.Update();
            }
            catch (Exception e)
            {
                Debug.LogError("Error during UI Update");
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// Perform rendering of a measurement.
        /// </summary>
        private void HandleMeasurement(RenderManager.CameraInfo cameraInfo, Measurement m)
        {
            // TODO: Move out of this slightly monolithic manager class

            if ((m.Flags & MeasurementFlags.Secondary) != 0 && !_secondaryDetailEnabled)
            {
                return;
            }

            if ((m.Flags & MeasurementFlags.Guide) != 0 && !SnapController.EnableAdvancedSnapping)
            {
                return;
            }

            if ((m.Flags & MeasurementFlags.Snap) != 0 && !SnapController.EnableSnapping)
            {
                return;
            }

            if (m is AngleMeasurement)
            {
                var am = m as AngleMeasurement;

                if (am.AngleSize < 1f)
                {
                    return;
                }

                AngleRenderer.Render(cameraInfo, am);

                var label = _ui.GetMeasurementLabel();
                label.SetValue(string.Format("{0:#.0}{1}", am.AngleSize.RoundToNearest(0.1f), "°"));
                label.SetWorldPosition(cameraInfo, AngleRenderer.GetLabelWorldPosition(am));

                return;
            }

            if (m is DistanceMeasurement)
            {
                var dm = m as DistanceMeasurement;

                if (Mathf.Abs(dm.Length) < Settings.MinimumDistanceMeasure)
                {
                    return;
                }

                DistanceRenderer.Render(cameraInfo, dm);

                var label = _ui.GetMeasurementLabel();

                string dist;

                if ((dm.Flags & MeasurementFlags.Height) != 0)
                {
                    dist = string.Format("H: {0}", StringUtil.GetHeightMeasurementString(dm.Length));
                }
                else
                {
                    dist = StringUtil.GetDistanceMeasurementString(dm.Length, _secondaryDetailEnabled);

                    if (_secondaryDetailEnabled)
                    {
                       var heightdiff = (int) dm.RelativeHeight.RoundToNearest(1);

                        if (Mathf.Abs(heightdiff) > 0)
                        {
                            dist += string.Format("\n(Elev: {0})", StringUtil.GetHeightMeasurementString(heightdiff));
                        }
                    }
                }

                label.SetValue(dist);
                label.SetWorldPosition(cameraInfo, DistanceRenderer.GetLabelWorldPosition(dm));

                return;
            }

            Debug.LogError("Measurement has no renderer: " + m);
        }
    }
}
