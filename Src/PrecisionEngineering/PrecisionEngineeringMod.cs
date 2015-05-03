using ICities;

namespace PrecisionEngineering
{
	public class PrecisionEngineeringMod : IUserMod
	{

		public string Name
		{
			get { return "Precision Engineering"; }
		}

		public string Description
		{
			get { return "Tools for creating roads with precision. Hold SHIFT to enable more information when placing roads."; }
		}

	}
}
