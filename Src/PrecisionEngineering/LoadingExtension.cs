using System;
using ICities;
using PrecisionEngineering.Detour;
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

            try
            {
                Debug.Log("OnLevelLoaded");

                Manager.OnLevelLoaded();

                FakeRoadAI.Deploy();
                SnapController.Deploy();
                AltKeyFix.Deploy();
            }
            catch (Exception e)
            {
                Debug.LogError("Error during OnLevelLoaded callback");
                Debug.LogError(e.ToString());
            }
        }

        public override void OnLevelUnloading()
        {
            try
            {
                Debug.Log("OnLevelUnloading");

                FakeRoadAI.Revert();
                SnapController.Revert();
                AltKeyFix.Revert();

                Manager.OnLevelUnloaded();
            }
            catch (Exception e)
            {
                Debug.LogError("Error during OnLevelUnloading callback");
                Debug.LogError(e.ToString());
            }

            base.OnLevelUnloading();
        }
    }
}
