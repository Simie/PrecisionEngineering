using ColossalFramework;
using ICities;
using PrecisionEngineering.Detour;
using PrecisionEngineering.UI;
using PrecisionEngineering.Utilities;
using UE = UnityEngine;

namespace PrecisionEngineering
{
	public class LoadingExtension : LoadingExtensionBase
	{

		public override void OnCreated(ILoading loading)
		{
			base.OnCreated(loading);
		}

		public override void OnLevelLoaded(LoadMode mode)
		{

			base.OnLevelLoaded(mode);

			Manager.OnLevelLoaded();

			Debug.Log("Detouring NetTool.SnapDirection()...");

            SnapController.StealControl();
			FakeRoadAI.Deploy();

		}

		public override void OnLevelUnloading()
		{

			Debug.Log("Returning control of NetTool.SnapDirection()...");

			SnapController.ReturnControl();
			FakeRoadAI.Revert();

            Manager.OnLevelUnloaded();

			base.OnLevelUnloading();

		}

	}
}
