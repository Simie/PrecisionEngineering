using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrecisionEngineering.Utilities
{
	public static class Util
	{

		public static void Swap<T>(ref T one, ref T two)
		{

			var t = one;
			one = two;
			two = t;

		}

	}
}
