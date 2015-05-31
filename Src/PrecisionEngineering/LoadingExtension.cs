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

			Debug.Log("OnLevelLoaded");

			Manager.OnLevelLoaded();

			FakeRoadAI.Deploy();
			SnapController.Deploy();
			AltKeyFix.Deploy();

		}

		public override void OnLevelUnloading()
		{

			Debug.Log("OnLevelUnloading");

			FakeRoadAI.Revert();
			SnapController.Revert();
			AltKeyFix.Revert();

			Manager.OnLevelUnloaded();

			base.OnLevelUnloading();

		}

	}
}
