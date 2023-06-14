using RGB.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models.Effects
{
    internal class RainbowEffect : AbstractEffectModel
    {
        private SingleValueSettingModel sat, val, speed, scale;
        public RainbowEffect()
        {
            Name = "Rainbow";
            IsDirty = true;
            Settings.Add(speed = new SingleValueSettingModel(this, "Speed", .01, 10, 1));
            Settings.Add(sat = new SingleValueSettingModel(this, "Saturation", 0, 1, 0));
            Settings.Add(val = new SingleValueSettingModel(this, "Brightness", 0, 1, 0));
            Settings.Add(scale = new SingleValueSettingModel(this, "Scale", 0, 5, .5));
        }

        double hue = 0;
        public override void GetColors(LedColor[] colors, bool hasWhite)
        {
            float r;
            float g;
            float b;
            hue += speed.Value;

            if (hasWhite)
                for (int i = 0; i < colors.Length; i++)
                {
                    ColorUtil.HsiToRgbw((hue + i * scale.Value) % 360, sat.Value, val.Value, out colors[i]);
                }
            else
                for (int i = 0; i < colors.Length; i++)
                {
                    ColorUtil.HsiToRgb((hue + i * scale.Value) % 360, sat.Value, val.Value, out colors[i]);
                }
        }
    }
}
