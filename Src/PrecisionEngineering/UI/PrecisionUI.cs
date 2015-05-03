using ColossalFramework.UI;

namespace PrecisionEngineering.UI
{
	class PrecisionUI : UICustomControl
	{


		public PrecisionUI(NetToolProxy netTool, PrecisionCalculator calculator)
		{

			var view = UIView.GetAView();

			var p = view.AddUIComponent(typeof (DebugUI)) as DebugUI;

			p.NetTool = netTool;
			p.Calculator = calculator;

		}

		
	}
}
