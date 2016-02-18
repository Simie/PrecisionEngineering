using System;
using UnityEngine;

namespace PrecisionEngineering
{
    internal static class ModSettings
    {
        public static int FontSize
        {
            get
            {
                if (!_fontSize.HasValue)
                {
                    _fontSize = PlayerPrefs.GetInt("PE_FONT_SIZE", 0);
                }

                return _fontSize.Value;
            }
            set
            {
                if(value > 2)
                    throw new ArgumentOutOfRangeException();

                if (value == _fontSize)
                {
                    return;
                }
                PlayerPrefs.SetInt("PE_FONT_SIZE", value);
                _fontSize = value;
            }
        }

        private static int? _fontSize;
    }
}