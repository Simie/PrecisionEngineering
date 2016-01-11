using System;
using ColossalFramework;
using UnityEngine;

namespace PrecisionEngineering.Utilities
{
    public static class NetUtil
    {
        public static bool AreSimilarClass(NetInfo i1, NetInfo i2)
        {
            if (i1 == i2)
            {
                return true;
            }

            if (i1 != null && i2 != null && i1.m_class.m_service == i2.m_class.m_service)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// A modified version of the NetManager.GetClosestSegments method which filters by the NetInfo class
        /// </summary>
        /// <param name="netInfo"></param>
        /// <param name="pos"></param>
        /// <param name="segments"></param>
        /// <param name="count"></param>
        public static void GetClosestSegments(NetInfo netInfo, Vector3 pos, ushort[] segments, out int count)
        {
            var nm = NetManager.instance;

            count = 0;
            var searchRange = 32f;

            for (var i = 0; i < 5; ++i)
            {
                var xMin = pos.x - searchRange;
                var zMin = pos.z - searchRange;
                var xMax = pos.x + searchRange;
                var zMax = pos.z + searchRange;

                var xTileMin = Mathf.Max((int) ((xMin - 64.0)/64.0 + 135.0), 0);
                var zTileMin = Mathf.Max((int) ((zMin - 64.0)/64.0 + 135.0), 0);
                var xTileMax = Mathf.Min((int) ((xMax + 64.0)/64.0 + 135.0), 269);
                var zTileMax = Mathf.Min((int) ((zMax + 64.0)/64.0 + 135.0), 269);

                for (var zTile = zTileMin; zTile <= zTileMax; ++zTile)
                {
                    for (var xTile = xTileMin; xTile <= xTileMax; ++xTile)
                    {
                        var segmentId = nm.m_segmentGrid[zTile*270 + xTile];
                        var num11 = 0;
                        while (segmentId != 0)
                        {
                            var startNodeId = nm.m_segments.m_buffer[segmentId].m_startNode;
                            var endNodeId = nm.m_segments.m_buffer[segmentId].m_endNode;
                            var startPosition = nm.m_nodes.m_buffer[startNodeId].m_position;
                            var endPosition = nm.m_nodes.m_buffer[endNodeId].m_position;
                            if (AreSimilarClass(nm.m_segments.m_buffer[segmentId].Info, netInfo) &&
                                (Mathf.Max(Mathf.Max(xMin - 64f - startPosition.x, zMin - 64f - startPosition.z),
                                    Mathf.Max((float) (startPosition.x - (double) xMax - 64.0),
                                        (float) (startPosition.z - (double) zMax - 64.0))) < 0.0 ||
                                 Mathf.Max(Mathf.Max(xMin - 64f - endPosition.x, zMin - 64f - endPosition.z),
                                     Mathf.Max((float) (endPosition.x - (double) xMax - 64.0),
                                         (float) (endPosition.z - (double) zMax - 64.0))) < 0.0) &&
                                Mathf.Min(startPosition.x, endPosition.x) <= (double) xMax &&
                                Mathf.Min(startPosition.z, endPosition.z) <= (double) zMax &&
                                Mathf.Max(startPosition.x, endPosition.x) >= (double) xMin &&
                                Mathf.Max(startPosition.z, endPosition.z) >= (double) zMin)
                            {
                                var isNew = true;
                                for (var j = 0; j < count; ++j)
                                {
                                    if (segments[j] == segmentId)
                                    {
                                        isNew = false;
                                        break;
                                    }
                                }
                                if (isNew)
                                {
                                    segments[count++] = segmentId;

                                    if (count == segments.Length)
                                    {
                                        return;
                                    }
                                }
                            }
                            segmentId = nm.m_segments.m_buffer[segmentId].m_nextGridSegment;

                            if (++num11 >= 32768)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core,
                                    "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                searchRange *= 2f;
            }
        }
    }
}
