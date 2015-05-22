using ICities;

namespace PrecisionEngineering
{
	public class Mod : IUserMod
	{

		public string Name
		{
			get { return "Precision Engineering"; }
		}

		public string Description
		{
			get { return "Build roads with precision. Hold CTRL to enable angle snapping, hold SHIFT to show more information."; }
		}

	}
}
