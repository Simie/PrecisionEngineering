using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;

namespace RoadEngineer
{
	public class PrecisionEngineeringMod : IUserMod
	{

		public string Name
		{
			get { return "Precision Engineering"; }
		}

		public string Description
		{
			get { return "Tools for creating roads with precision."; }
		}

	}
}
