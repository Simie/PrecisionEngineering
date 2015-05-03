using System.Collections.Generic;
using ColossalFramework.UI;
using PrecisionEngineering.Data;

namespace PrecisionEngineering.UI
{
	class PrecisionUI : UICustomControl
	{

		private UIView _rootView;

		private readonly List<MeasurementLabel> _activeAngleLabels = new List<MeasurementLabel>();
		private readonly List<MeasurementLabel> _angleLabelPool = new List<MeasurementLabel>();

		public PrecisionUI()
		{

			_rootView = UIView.GetAView();

		}

		public void CreateDebugUI(NetToolProxy netTool, PrecisionCalculator calc)
		{

			var p = _rootView.AddUIComponent(typeof(DebugUI)) as DebugUI;

			p.NetTool = netTool;
			p.Calculator = calc;

		}

		public void ReleaseAll()
		{

			for (var i = _activeAngleLabels.Count - 1; i >= 0; i--) {

				_angleLabelPool.Add(_activeAngleLabels[i]);
				_activeAngleLabels[i].isVisible = false;

			}

			_activeAngleLabels.Clear();

		}

		public MeasurementLabel GetMeasurementLabel()
		{

			MeasurementLabel l;

			if (_angleLabelPool.Count == 0) {

				l = CreateMeasurementLabel();

			} else {

				l = _angleLabelPool[_angleLabelPool.Count - 1];
				_angleLabelPool.RemoveAt(_angleLabelPool.Count - 1);

			}

			_activeAngleLabels.Add(l);
			l.isVisible = true;

			return l;

		}

		MeasurementLabel CreateMeasurementLabel()
		{

			return _rootView.AddUIComponent(typeof (MeasurementLabel)) as MeasurementLabel;

		}
		
	}
}
