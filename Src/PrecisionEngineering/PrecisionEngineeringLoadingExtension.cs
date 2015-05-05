using ColossalFramework;
using ICities;
using PrecisionEngineering.UI;
using PrecisionEngineering.Utilities;
using UE = UnityEngine;

namespace PrecisionEngineering
{
	public class PrecisionEngineeringLoadingExtension : LoadingExtensionBase
	{

		public override void OnCreated(ILoading loading)
		{

			base.OnCreated(loading);

			Debug.Log("Detouring NetTool.SnapDirection()...");

			SnapController.StealControl();

		}

		public override void OnLevelLoaded(LoadMode mode)
		{

			base.OnLevelLoaded(mode);

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
