using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrecisionEngineering.Utilities
{
	public static class NetUtil
	{

		public static bool AreSimilarClass(NetInfo i1, NetInfo i2)
		{

			if (i1 == i2)
				return true;

			if (i1 != null && i2 != null && i1.m_class.m_service == i2.m_class.m_service)
				return true;

			return false;

		}

	}
}
