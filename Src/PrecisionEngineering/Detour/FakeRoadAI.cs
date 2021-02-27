using System;
using System.Reflection;

namespace PrecisionEngineering.Detour
{
    /// <summary>
    /// Detouring some RoadAI methods to tweak how snapping is handled when advanced snapping is enabled.
    /// </summary>
    internal static class FakeRoadAI
    {
        private static RedirectCallsState _revertState;
        private static readonly MethodInfo _originalGetLengthSnapMethodInfo = typeof (RoadAI).GetMethod("GetLengthSnap");
        private static readonly MethodInfo _newGetLengthSnapMethodInfo = typeof (FakeRoadAI).GetMethod("GetLengthSnap");

        public static void Deploy()
        {
            if (_originalGetLengthSnapMethodInfo == null)
            {
                throw new NullReferenceException("Original GetLengthSnap method not found");
            }

            if (_newGetLengthSnapMethodInfo == null)
            {
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
            if (!SnapController.EnableLengthSnapping)
            {
                return 0f;
            }

            // No net tool overrides this value any more - just return the default.
            return 8f; 
        }
    }
}
