using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering
{
	class SnapController
	{

		private static readonly MethodInfo SnapDirectionOriginalMethodInfo = typeof (NetTool).GetMethod("SnapDirection");
		private static readonly MethodInfo SnapDirectionOverrideMethodInfo = typeof (SnapController).GetMethod("SnapDirectionOverride");

		private static RedirectCallsState _revertState;
		private static bool _hasControl;

		static SnapController()
		{
			
		}

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

		public static bool EnableSnapping;

		public static NetTool.ControlPoint SnapDirectionOverride(NetTool.ControlPoint newPoint, NetTool.ControlPoint oldPoint,
			NetInfo info, out bool success, out float minDistanceSq)
		{

			// Quick bypass if custom snapping is disabled - jump to the original CS implementation
			if (!EnableSnapping) {

				goto Original;

			}

			minDistanceSq = info.GetMinNodeDistance();
			minDistanceSq = minDistanceSq * minDistanceSq;
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

				// Direction and length of the line from the segment branch point to the users control point
				var userLineDirection = (newPoint.m_position - oldPoint.m_position).Flatten();
				var userLineLength = userLineDirection.magnitude;
				userLineDirection.Normalize();

				Vector3 segmentDirection;
				Vector3 segmentPosition;

				// Get direction of the segment at the branch position
				sourceSegment.GetClosestPositionAndDirection(oldPoint.m_position, out segmentPosition, out segmentDirection);

				var currentAngle = Vector3Extensions.Angle(segmentDirection, userLineDirection, Vector3.up);

				segmentDirection = segmentDirection.Flatten().normalized;

				var snappedAngle = Mathf.Round(currentAngle / Settings.SnapAngle) * Settings.SnapAngle;
				var snappedDirection = Quaternion.AngleAxis(snappedAngle, Vector3.up) * segmentDirection;

				controlPoint.m_direction = snappedDirection.normalized;
				controlPoint.m_position = oldPoint.m_position + userLineLength * controlPoint.m_direction;
				controlPoint.m_position.y = newPoint.m_position.y;

				minDistanceSq = (newPoint.m_position - controlPoint.m_position).sqrMagnitude;

				success = true;

			}

			if(success)
				return controlPoint;

			Original:

			ReturnControl();

			var result = NetTool.SnapDirection(newPoint, oldPoint, info, out success, out minDistanceSq);

			StealControl();

			return result;

		}

	}
}
