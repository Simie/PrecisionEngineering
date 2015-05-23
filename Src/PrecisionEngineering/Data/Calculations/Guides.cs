using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Data.Calculations
{
	internal static class Guides
	{

		private static readonly ushort[] SegmentCache = new ushort[32];
		private static int SegmentCacheCount;

		public static void CalculateGuideLines(NetInfo netInfo, NetTool.ControlPoint startPoint, NetTool.ControlPoint endPoint, IList<GuideLine> resultList)
		{

			var startPosition = startPoint.m_position;
			var endPosition = endPoint.m_position;

			NetManager.instance.GetClosestSegments(endPosition, SegmentCache, out SegmentCacheCount);

			for (var i = 0; i < SegmentCacheCount; i++) {

				var segmentId = SegmentCache[i];

				var s = NetManager.instance.m_segments.m_buffer[segmentId];

				// Ensure they are part of the same network
				if (!NetUtil.AreSimilarClass(s.Info, netInfo))
					continue;

				// Test the start and end of the segment

				// Check if the node can branch in the guide direction (angles less than 45deg or so should be discarded)
				if (CanNodeBranchInDirection(s.m_endNode, s.m_startDirection)) {

					var endNode = NetManager.instance.m_nodes.m_buffer[s.m_endNode];

					TestLine(s.Info, startPosition, endPosition,
						endNode.m_position,
						endNode.m_position + s.m_startDirection.Flatten(),
						resultList, segmentId, s.m_endNode);

				}

				if (CanNodeBranchInDirection(s.m_startNode, s.m_endDirection)) {

					var startNode = NetManager.instance.m_nodes.m_buffer[s.m_startNode];

					TestLine(s.Info, startPosition, endPosition,
						startNode.m_position,
						startNode.m_position + s.m_endDirection.Flatten(),
						resultList, segmentId, s.m_startNode);

				}
			}

		}

		private static void TestLine(NetInfo netInfo, Vector3 startPoint, Vector3 endPoint, Vector3 l1, Vector3 l2, IList<GuideLine> lines, ushort segmentId = 0, ushort nodeId = 0)
		{

			var p = Util.LineIntersectionPoint(startPoint.xz(), endPoint.xz(), l1.xz(), l2.xz());

			// Lines never meet, discard
			if (!p.HasValue)
				return;

			var intersect = new Vector3(p.Value.x, 0, p.Value.y);

			var guideDirection = l1.Flatten().DirectionTo(l2.Flatten());
			var intersectDirection = l1.Flatten().DirectionTo(intersect);

			// Ignore guides in the wrong direction
			if (Mathf.Abs(Vector3Extensions.GetSignedAngleBetween(guideDirection, intersectDirection, Vector3.up)) > 90f)
				return;

			intersect.y = TerrainManager.instance.SampleRawHeightSmooth(intersect);

			var intersectDistance = Vector3.Distance(endPoint, intersect);

			var line = new GuideLine(l1, intersect, netInfo.m_halfWidth * 2f, intersectDistance);

			int index;

			if (IsDuplicate(lines, line, out index)) {

				var d = lines[index];

				// If a duplicate, check if it is closer than the duplicate
				if (Vector3Extensions.DistanceSquared(d.Origin, endPoint) > Vector3Extensions.DistanceSquared(l1, endPoint)) {
					lines[index] = line;
				}

				return;

			}

			// Check for intersection with existing nodes
			var ra = Vector3.Cross(guideDirection, Vector3.up);
			var quad = new Quad3(l1 + ra, l1 - ra, intersect + ra, intersect - ra);

			if (NetManager.instance.OverlapQuad(Quad2.XZ(quad), 0, 1280f, netInfo.m_class.m_layer, nodeId, 0, segmentId)) {
				return;
			}

			lines.Add(line);

		}

		public static bool IsDuplicate(IList<GuideLine> existingLines, GuideLine newLine, out int index)
		{

			// Discard if an existing line has the same direction and origin
			for (var i = 0; i < existingLines.Count; i++) {

				var existingLine = existingLines[i];

				if (Vector3.Dot(existingLine.Direction.Flatten(), newLine.Direction.Flatten()) < 0.9f)
					continue;

				if (Vector3Extensions.DistanceSquared(existingLine.Intersect, newLine.Intersect) < 0.5f) {
					index = i;
					return true;
				}

			}

			index = -1;
			return false;

		}

		private static bool CanNodeBranchInDirection( ushort nodeId, Vector3 direction)
		{

			var closestSegmentId = NetNodeUtility.GetClosestSegmentId(nodeId, direction);
			
			var exitDirection = NetNodeUtility.GetSegmentExitDirection(nodeId, closestSegmentId);

			var exitAngle = Vector3Extensions.GetSignedAngleBetween(direction.Flatten(), exitDirection.Flatten(), Vector3.up);

			return Mathf.Abs(exitAngle) >= 45f;

		}

	}
}
