using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PrecisionEngineering.Detour
{

	static class FakeRoadAI
	{

		public static bool DisableLengthSnap = false;

		private static RedirectCallsState _revertState;
		private static readonly MethodInfo _originalGetLengthSnapMethodInfo = typeof(RoadAI).GetMethod("GetLengthSnap");
		private static readonly MethodInfo _newGetLengthSnapMethodInfo = typeof(FakeRoadAI).GetMethod("GetLengthSnap");

		public static void Deploy()
		{

			if (_originalGetLengthSnapMethodInfo == null) {
				throw new NullReferenceException("Original GetLengthSnap method not found");
			}

			if (_newGetLengthSnapMethodInfo == null) {
				throw new NullReferenceException("New GetLengthSnap method not found");
			}

			_revertState = RedirectionHelper.RedirectCalls(_originalGetLengthSnapMethodInfo,
				_newGetLengthSnapMethodInfo);
		}

		public static void Revert()
		{
			RedirectionHelper.RevertRedirect(_originalGetLengthSnapMethodInfo, _revertState);
		}

		public static float GetLengthSnap(RoadAI roadAi)
		{

			if (DisableLengthSnap)
				return 0f;

			return roadAi.m_enableZoning ? 8f : 0f;

		}

	}

}
