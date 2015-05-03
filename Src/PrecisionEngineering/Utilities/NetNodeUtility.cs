using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrecisionEngineering.Utilities
{
	public static class NetNodeUtility
	{

		public static List<NetSegment> GetNodeSegments(NetNode node)
		{

			var list = new List<NetSegment>(node.CountSegments());

			if (node.m_segment0 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment0]);
			if (node.m_segment1 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment1]);
			if (node.m_segment2 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment2]);
			if (node.m_segment3 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment3]);
			if (node.m_segment4 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment4]);
			if (node.m_segment5 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment5]);
			if (node.m_segment6 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment6]);
			if (node.m_segment7 > 0)
				list.Add(NetManager.instance.m_segments.m_buffer[node.m_segment7]);

			return list;

		}

		public static List<ushort> GetNodeSegmentIds(NetNode node)
		{

			var list = new List<ushort>(node.CountSegments());

			if (node.m_segment0 > 0)
				list.Add(node.m_segment0);
			if (node.m_segment1 > 0)
				list.Add(node.m_segment1);
			if (node.m_segment2 > 0)
				list.Add(node.m_segment2);
			if (node.m_segment3 > 0)
				list.Add(node.m_segment3);
			if (node.m_segment4 > 0)
				list.Add(node.m_segment4);
			if (node.m_segment5 > 0)
				list.Add(node.m_segment5);
			if (node.m_segment6 > 0)
				list.Add(node.m_segment6);
			if (node.m_segment7 > 0)
				list.Add(node.m_segment7);

			return list;

		}

	}
}
