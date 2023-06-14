using RGB.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models.Effects
{
    internal class StaticEffect : AbstractEffectModel
    {
        private ColorSettingModel selectedColor;
        public StaticEffect()
        {
            Name = "Static Color";
            IsDirty = true;
            
            Settings.Add(selectedColor = new ColorSettingModel(this, "Color"));
        }

        public override void GetColors(LedColor[] colors, bool hasWhite)
        {
            LedColor color;
            if (hasWhite)
                color = new LedColor((float)selectedColor.Red, (float)selectedColor.Green, (float)selectedColor.Blue, (float)selectedColor.White);
            else
                ColorUtil.RgbwToRgb(selectedColor, out color);

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            IsDirty = false;
        }
    }
}
