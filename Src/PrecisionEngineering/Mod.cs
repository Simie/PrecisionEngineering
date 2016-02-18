using System;
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
            get
            {
                return
                    "Build with precision. Hold CTRL to enable angle snapping, SHIFT to show more information, ALT to snap to guide-lines.";
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var group = helper.AddGroup("UI");
            group.AddDropdown("Font Size", new [] {"Normal", "Large", "X-Large"}, ModSettings.FontSize,
                OnFontSizeChanged);
        }

        private void OnFontSizeChanged(int val)
        {
            ModSettings.FontSize = val;
        }
    }
}
