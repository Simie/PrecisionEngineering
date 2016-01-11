using System.Collections.Generic;
using ColossalFramework.UI;
using PrecisionEngineering.Data;

namespace PrecisionEngineering.UI
{
    internal class PrecisionUI
    {
        private readonly List<MeasurementLabel> _activeAngleLabels = new List<MeasurementLabel>();
        private readonly List<MeasurementLabel> _angleLabelPool = new List<MeasurementLabel>();
        private DebugUI _debugUi;

        private UIView _rootView;

        public NetToolProxy NetToolProxy { get; set; }
        public PrecisionCalculator Calculator { get; set; }

        private void Load()
        {
            Debug.Log("[PrecisionUI] Load");

            _rootView = UIView.GetAView();

            // When reloading a game, all of this will be destroyed anyway
            _activeAngleLabels.Clear();
            _angleLabelPool.Clear();

            if (Debug.Enabled)
            {
                CreateDebugUI();
            }
        }

        public void CreateDebugUI()
        {
            if (_rootView == null)
            {
                Load();
            }

            _debugUi = _rootView.AddUIComponent(typeof (DebugUI)) as DebugUI;
        }

        public void ReleaseAll()
        {
            if (_rootView == null)
            {
                Load();
            }

            for (var i = _activeAngleLabels.Count - 1; i >= 0; i--)
            {
                _angleLabelPool.Add(_activeAngleLabels[i]);
                _activeAngleLabels[i].isVisible = false;
            }

            _activeAngleLabels.Clear();
        }

        public MeasurementLabel GetMeasurementLabel()
        {
            if (_rootView == null)
            {
                Load();
            }

            MeasurementLabel l;

            if (_angleLabelPool.Count == 0)
            {
                l = CreateMeasurementLabel();
            }
            else
            {
                l = _angleLabelPool[_angleLabelPool.Count - 1];
                _angleLabelPool.RemoveAt(_angleLabelPool.Count - 1);
            }

            _activeAngleLabels.Add(l);
            l.isVisible = true;

            return l;
        }

        private MeasurementLabel CreateMeasurementLabel()
        {
            return _rootView.AddUIComponent(typeof (MeasurementLabel)) as MeasurementLabel;
        }

        public void Update()
        {
            if (_debugUi != null)
            {
                _debugUi.Calculator = Calculator;
                _debugUi.NetTool = NetToolProxy;
                _debugUi.DoUpdate();
            }
        }
    }
}
