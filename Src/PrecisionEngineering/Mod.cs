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
            get { return "Build with precision. Hold CTRL to enable angle snapping, SHIFT to show more information, ALT to snap to guide-lines."; }
        }

    }
}
