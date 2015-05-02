using ICities;
using UE = UnityEngine;

namespace PrecisionEngineering
{
	public class LoadingExtension : LoadingExtensionBase
	{

		public override void OnLevelLoaded(LoadMode mode)
		{

			base.OnLevelLoaded(mode);

			var netTool = UE.Object.FindObjectOfType<NetTool>();
			


		}

	}
}
