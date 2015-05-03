using System.Collections.Generic;
using ColossalFramework.UI;

namespace PrecisionEngineering.UI
{
	class PrecisionUI : UICustomControl
	{

		private UIView _rootView;

		private readonly List<AngleLabel> _activeAngleLabels = new List<AngleLabel>();
		private readonly List<AngleLabel> _angleLabelPool = new List<AngleLabel>();

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

		public AngleLabel GetAngleLabel()
		{

			AngleLabel l;

			if (_angleLabelPool.Count == 0) {

				l = CreateAngleLabel();

			} else {

				l = _angleLabelPool[_angleLabelPool.Count - 1];
				_angleLabelPool.RemoveAt(_angleLabelPool.Count - 1);

			}

			_activeAngleLabels.Add(l);
			l.isVisible = true;

			return l;

		}

		AngleLabel CreateAngleLabel()
		{

			return _rootView.AddUIComponent(typeof (AngleLabel)) as AngleLabel;

		}
		
	}
}
