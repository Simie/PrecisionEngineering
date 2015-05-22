using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ColossalFramework.Math;
using PrecisionEngineering.Utilities;
using UnityEngine;

namespace PrecisionEngineering.Data.Calculations
{
	internal static class Guides
	{

		private static readonly ushort[] SegmentCache = new ushort[32];

		public static void CalculateGuideLines(NetInfo netInfo, NetTool.ControlPoint startPoint, NetTool.ControlPoint endPoint, IList<GuideLine> resultList)
		{

			int count;

			var startPosition = startPoint.m_position;
			var endPosition = endPoint.m_position;

			NetManager.instance.GetClosestSegments(endPosition, SegmentCache, out count);

			for (var i = 0; i < count; i++) {

				var s = NetManager.instance.m_segments.m_buffer[SegmentCache[i]];

				// Ensure they are part of the same network
				if (!NetUtil.AreSimilarClass(s.Info, netInfo))
					continue;

				/*Vector3 pos;
				Vector3 dir;

				s.GetClosestPositionAndDirection(endPosition, out pos, out dir);

				var pos2 = pos + dir.Flatten();

				//TestLine(s.Info, startPosition, endPosition, pos, pos2, resultList);*/

				TestLine(s.Info, startPosition, endPosition,
					NetManager.instance.m_nodes.m_buffer[s.m_endNode].m_position,
					NetManager.instance.m_nodes.m_buffer[s.m_endNode].m_position + s.m_endDirection.Flatten(),
					resultList);

				TestLine(s.Info, startPosition, endPosition,
					NetManager.instance.m_nodes.m_buffer[s.m_startNode].m_position,
					NetManager.instance.m_nodes.m_buffer[s.m_startNode].m_position + s.m_startDirection.Flatten(),
					resultList);

			}

		}

		private static void TestLine(NetInfo netInfo, Vector3 startPoint, Vector3 endPoint, Vector3 l1, Vector3 l2, IList<GuideLine> lines)
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

			var intersectDistance = Vector3.Distance(endPoint, intersect);

			intersect.y = TerrainManager.instance.SampleRawHeightSmooth(intersect);

			var line = new GuideLine(l1, intersect, netInfo.m_halfWidth * 2f, intersectDistance);

			if (IsDuplicate(lines, line)) {
				return;
			}

			lines.Add(line);

		}

		public static bool IsDuplicate(IList<GuideLine> existingLines, GuideLine newLine)
		{

			// Discard if an existing line has the same direction and origin
			for (var i = 0; i < existingLines.Count; i++) {

				var existingLine = existingLines[i];

				if (Vector3.Dot(existingLine.Direction.Flatten(), newLine.Direction.Flatten()) < 0.9f)
					continue;

				if (Vector3Extensions.DistanceSquared(existingLine.Intersect, newLine.Intersect) < 0.5f)
					return true;

			}

			return false;

		}

	}
}
