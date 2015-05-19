using ColossalFramework;
using ICities;
using PrecisionEngineering.UI;
using PrecisionEngineering.Utilities;
using UE = UnityEngine;

namespace PrecisionEngineering
{
	public class PrecisionEngineeringLoadingExtension : LoadingExtensionBase
	{

		public override void OnLevelLoaded(LoadMode mode)
		{

			base.OnLevelLoaded(mode);
            
            Debug.Log("Detouring NetTool.SnapDirection()...");

            SnapController.StealControl();

            SimulationManager.RegisterManager(PrecisionEngineeringManager.instance);

        }

		public override void OnLevelUnloading()
		{

			Debug.Log("Returning control of NetTool.SnapDirection()...");

			SnapController.ReturnControl();

			base.OnLevelUnloading();

		}

	}
}
