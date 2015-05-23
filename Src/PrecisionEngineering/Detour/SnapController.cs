using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework.UI;
using PrecisionEngineering.Data;
using PrecisionEngineering.Data.Calculations;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Detour
{
	internal class SnapController
	{

		public static bool EnableSnapping;
		public static bool EnableAdvancedSnapping;

		public static GuideLine? SnappedGuideLine;
		public static readonly IList<GuideLine> GuideLines = new List<GuideLine>();  

		public static string DebugPrint = "";

		private static readonly MethodInfo SnapDirectionOriginalMethodInfo = typeof (NetTool).GetMethod("SnapDirection");

		private static readonly MethodInfo SnapDirectionOverrideMethodInfo =
			typeof (SnapController).GetMethod("SnapDirectionOverride");

		private static RedirectCallsState _revertState;
		private static bool _hasControl;

		static SnapController() {}

		public static void StealControl()
		{

			if (_hasControl)
				return;

			_revertState = RedirectionHelper.RedirectCalls(SnapDirectionOriginalMethodInfo, SnapDirectionOverrideMethodInfo);
			_hasControl = true;

		}

		public static void ReturnControl()
		{

			if (!_hasControl)
				return;

			RedirectionHelper.RevertRedirect(SnapDirectionOriginalMethodInfo, _revertState);
			_hasControl = false;
		}

		public static NetTool.ControlPoint SnapDirectionOverride(NetTool.ControlPoint newPoint, NetTool.ControlPoint oldPoint,
			NetInfo info, out bool success, out float minDistanceSq)
		{

			if (Debug.Enabled) {

				DebugPrint = string.Format("oldPoint: {0}\nnewPoint:{1}", StringUtil.ToString(oldPoint),
					StringUtil.ToString(newPoint));

			}

			GuideLines.Clear();
			SnappedGuideLine = null;

			// Quick bypass if custom snapping is disabled - jump to the original CS implementation
			if (!EnableSnapping) {

				goto Original;

			}

			minDistanceSq = info.GetMinNodeDistance();
			minDistanceSq = minDistanceSq*minDistanceSq;
			var controlPoint = newPoint;
			success = false;

			// If dragging from a node
			if (oldPoint.m_node != 0 && !newPoint.m_outside) {

				// Node the road build operation is starting from
				var sourceNodeId = oldPoint.m_node;
				var sourceNode = NetManager.instance.m_nodes.m_buffer[sourceNodeId];

				// Direction and length of the line from the node to the users control point
				var userLineDirection = (newPoint.m_position - sourceNode.m_position).Flatten();
				var userLineLength = userLineDirection.magnitude;
				userLineDirection.Normalize();

				var closestSegmentId = NetNodeUtility.GetClosestSegmentId(sourceNodeId, userLineDirection);

				if (closestSegmentId > 0) {

					// Snap to angle increments originating from this closest segment

					var closestSegmentDirection = NetNodeUtility.GetSegmentExitDirection(sourceNodeId, closestSegmentId);

					var currentAngle = Vector3Extensions.Angle(closestSegmentDirection, userLineDirection, Vector3.up);

					var snappedAngle = Mathf.Round(currentAngle/Settings.SnapAngle)*Settings.SnapAngle;
					var snappedDirection = Quaternion.AngleAxis(snappedAngle, Vector3.up)*closestSegmentDirection;

					controlPoint.m_direction = snappedDirection.normalized;
					controlPoint.m_position = sourceNode.m_position + userLineLength*controlPoint.m_direction;
					controlPoint.m_position.y = newPoint.m_position.y;

					minDistanceSq = (newPoint.m_position - controlPoint.m_position).sqrMagnitude;
					success = true;

					//minDistanceSq = olpo;


				}

			} else if (oldPoint.m_segment != 0 && !newPoint.m_outside) {

				// Else if dragging from a segment

				// Segment the road build operation is starting from
				var sourceSegmentId = oldPoint.m_segment;
				var sourceSegment = NetManager.instance.m_segments.m_buffer[sourceSegmentId];

				Vector3 segmentDirection;
				Vector3 segmentPosition;

				// Direction and length of the line between control points
				var userLineDirection = (newPoint.m_position - oldPoint.m_position).Flatten();
				var userLineLength = userLineDirection.magnitude;
				userLineDirection.Normalize();

				// Get direction of the segment at the branch position
				sourceSegment.GetClosestPositionAndDirection(oldPoint.m_position, out segmentPosition, out segmentDirection);

				var currentAngle = Vector3Extensions.Angle(segmentDirection, userLineDirection, Vector3.up);

				segmentDirection = segmentDirection.Flatten().normalized;

				var snappedAngle = Mathf.Round(currentAngle/Settings.SnapAngle)*Settings.SnapAngle;
				var snappedDirection = Quaternion.AngleAxis(snappedAngle, Vector3.up)*segmentDirection;

				controlPoint.m_direction = snappedDirection.normalized;
				controlPoint.m_position = oldPoint.m_position + userLineLength*controlPoint.m_direction;
				controlPoint.m_position.y = newPoint.m_position.y;

				minDistanceSq = (newPoint.m_position - controlPoint.m_position).sqrMagnitude;

				success = true;

			} else if (oldPoint.m_direction.sqrMagnitude > 0.5f) {

				if (newPoint.m_node == 0 && !newPoint.m_outside) {

					// Let's do some snapping between control point directions

					var currentAngle = Vector3Extensions.Angle(oldPoint.m_direction, newPoint.m_direction, Vector3.up);

					var snappedAngle = Mathf.Round(currentAngle/Settings.SnapAngle)*Settings.SnapAngle;
					var snappedDirection = Quaternion.AngleAxis(snappedAngle, Vector3.up)*oldPoint.m_direction.Flatten();

					controlPoint.m_direction = snappedDirection.normalized;

					controlPoint.m_position = oldPoint.m_position +
					                          Vector3.Distance(oldPoint.m_position.Flatten(), newPoint.m_position.Flatten())*
					                          controlPoint.m_direction;

					controlPoint.m_position.y = newPoint.m_position.y;

					success = true;

				}

			}

			if (EnableAdvancedSnapping) {

				if (controlPoint.m_segment == 0 && controlPoint.m_node == 0) {

					controlPoint = SnapDirectionGuideLines(controlPoint, oldPoint, info, ref success, ref minDistanceSq);

				}

			}

			if (success)
				return controlPoint;

			Original:

			ReturnControl();

			var result = NetTool.SnapDirection(newPoint, oldPoint, info, out success, out minDistanceSq);

			StealControl();

			return result;

		}

		public static NetTool.ControlPoint SnapDirectionGuideLines(NetTool.ControlPoint newPoint, NetTool.ControlPoint oldPoint,
			NetInfo info, ref bool success, ref float minDistanceSq)
		{

			var controlPoint = newPoint;

			Guides.CalculateGuideLines(info, oldPoint, controlPoint, GuideLines);

			if (GuideLines.Count == 0) {

				if(Debug.Enabled)
					DebugPrint += " (No GuideLines Found)";

				return newPoint;
			}

			var lines =
				GuideLines.OrderBy(p => p.Distance)
				          .ThenBy(p => Vector3.Distance(p.Origin, newPoint.m_position));

			var closestLine = lines.First();

			if (closestLine.Distance <= Settings.GuideLinesSnapDistance + closestLine.Width) {

				if (Debug.Enabled) {
					DebugPrint += " Guide: " + closestLine.Intersect.ToString();
				}

				controlPoint.m_position = closestLine.Intersect;
				controlPoint.m_position.y = newPoint.m_position.y;
				controlPoint.m_direction = oldPoint.m_position.DirectionTo(newPoint.m_position);
				success = true;

				SnappedGuideLine = closestLine;
				FakeRoadAI.DisableLengthSnap = true;

			}

			return controlPoint;

		}

	}
}
