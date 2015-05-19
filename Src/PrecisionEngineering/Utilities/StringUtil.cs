using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrecisionEngineering.Utilities
{
	internal static class StringUtil
	{

		public static string ToString(NetTool.ControlPoint pt)
		{
			return string.Format("\tPosition: {0}, Direction: {1}\n\t\t, Node: {2}, Segment: {3}", pt.m_position,
				pt.m_direction, pt.m_node, pt.m_segment);
		}

		public static string ToString(NetTool.NodePosition pt)
		{
			return string.Format("\tPosition: {0}, Direction: {1}", pt.m_position, pt.m_direction);
		}

	}
}
