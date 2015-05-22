using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrecisionEngineering.Detour
{

	static class FakeRoadAI
	{

		public static bool DisableLengthSnap = false;

		private static RedirectCallsState _revertState;

		public static void Deploy()
		{
			_revertState = RedirectionHelper.RedirectCalls(typeof (RoadAI).GetMethod("GetLengthSnap"),
				typeof (FakeRoadAI).GetMethod("GetLengthSnap"));
		}

		public static void Revert()
		{
			RedirectionHelper.RevertRedirect(typeof (RoadAI).GetMethod("GetLengthSnap"), _revertState);
		}

		public static float GetLengthSnap(this RoadAI roadAi)
		{

			if (DisableLengthSnap)
				return 0f;

			return roadAi.m_enableZoning ? 8f : 0f;

		}

	}

}
