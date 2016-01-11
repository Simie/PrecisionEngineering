using System.Collections.Generic;
using UnityEngine;

namespace PrecisionEngineering.Utilities
{
    public static class NetNodeUtility
    {
        private static readonly List<ushort> _segmentIdCache = new List<ushort>();

        public static List<NetSegment> GetNodeSegments(NetNode node)
        {
            var list = new List<NetSegment>(node.CountSegments());

            GetNodeSegments(node, list);

            return list;
        }

        public static List<ushort> GetNodeSegmentIds(NetNode node)
        {
            var list = new List<ushort>(node.CountSegments());

            GetNodeSegmentIds(node, list);

            return list;
        }

        public static void GetNodeSegments(NetNode node, IList<NetSegment> list)
        {
            if (node.m_segment0 > 0)
            {
                list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment0]);
            }
            if (node.m_segment1 > 0)
            {
                list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment1]);
            }
            if (node.m_segment2 > 0)
            {
                list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment2]);
            }
            if (node.m_segment3 > 0)
            {
                list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment3]);
            }
            if (node.m_segment4 > 0)
            {
                list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment4]);
            }
            if (node.m_segment5 > 0)
            {
                list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment5]);
            }
            if (node.m_segment6 > 0)
            {
                list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment6]);
            }
            if (node.m_segment7 > 0)
            {
                list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment7]);
            }
        }

        public static void GetNodeSegmentIds(NetNode node, IList<ushort> list)
        {
            if (node.m_segment0 > 0)
            {
                list.Add(node.m_segment0);
            }
            if (node.m_segment1 > 0)
            {
                list.Add(node.m_segment1);
            }
            if (node.m_segment2 > 0)
            {
                list.Add(node.m_segment2);
            }
            if (node.m_segment3 > 0)
            {
                list.Add(node.m_segment3);
            }
            if (node.m_segment4 > 0)
            {
                list.Add(node.m_segment4);
            }
            if (node.m_segment5 > 0)
            {
                list.Add(node.m_segment5);
            }
            if (node.m_segment6 > 0)
            {
                list.Add(node.m_segment6);
            }
            if (node.m_segment7 > 0)
            {
                list.Add(node.m_segment7);
            }
        }

        /// <summary>
        /// Get the direction from a node that a segment is going
        /// </summary>
        /// <param name="nodeId">Origin node</param>
        /// <param name="segmentId">Segment branching from the node</param>
        /// <returns>Direction vector</returns>
        public static Vector3 GetSegmentExitDirection(ushort nodeId, ushort segmentId)
        {
            var segment = NetManager.instance.m_segments.m_buffer[segmentId];

            var v = nodeId == segment.m_startNode ? segment.m_startDirection : segment.m_endDirection;

            return v;
        }

        public static ushort GetClosestSegmentId(ushort nodeId, Vector3 direction)
        {
            var closestAngle = 360f;
            ushort closestSegmentId = 0;

            _segmentIdCache.Clear();
            GetNodeSegmentIds(NetManager.instance.m_nodes.m_buffer[nodeId], _segmentIdCache);

            for (var i = 0; i < _segmentIdCache.Count; i++)
            {
                var d = GetSegmentExitDirection(nodeId, _segmentIdCache[i]);

                var a = Vector3Extensions.GetSignedAngleBetween(direction, d, Vector3.up);
                a = Mathf.Abs(a);

                if (a < closestAngle)
                {
                    closestSegmentId = _segmentIdCache[i];
                    closestAngle = a;
                }
            }

            return closestSegmentId;
        }
    }
}
