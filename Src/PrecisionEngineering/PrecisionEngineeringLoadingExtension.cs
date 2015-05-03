using ColossalFramework;
using ICities;
using PrecisionEngineering.UI;
using UE = UnityEngine;

namespace PrecisionEngineering
{
	public class PrecisionEngineeringLoadingExtension : LoadingExtensionBase
	{

		public override void OnLevelLoaded(LoadMode mode)
		{

			base.OnLevelLoaded(mode);

			SimulationManager.RegisterManager(PrecisionEngineeringManager.instance);

		}

	}
}
