using RGB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Util
{
    internal static class ColorUtil
    {
        // https://blog.saikoled.com/post/43693602826/why-every-led-light-should-be-using-hsi
        public static void HsiToRgb(double H, double S, double I, out LedColor color)
        {
            double r, g, b;
            H %= 360; // cycle H around to 0-360 degrees
            H = Math.PI * H / 180.0; // Convert to radians.
            S = S > 0 ? (S < 1 ? S : 1) : 0; // clamp S and I to interval [0,1]
            I = I > 0 ? (I < 1 ? I : 1) : 0;

            // Math! Thanks in part to Kyle Miller.
            if (H < Math.PI / 1.5)
            {
                r = I / 3 * (1 + S * Math.Cos(H) / Math.Cos(Math.PI / 3 - H));
                g = I / 3 * (1 + S * (1 - Math.Cos(H) / Math.Cos(Math.PI / 3 - H)));
                b = I / 3 * (1 - S);
            }
            else if (H < Math.PI / .75)
            {
                H = H - Math.PI / 1.5;
                g = I / 3 * (1 + S * Math.Cos(H) / Math.Cos(Math.PI / 3 - H));
                b = I / 3 * (1 + S * (1 - Math.Cos(H) / Math.Cos(Math.PI / 3 - H)));
                r = I / 3 * (1 - S);
            }
            else
            {
                H = H - Math.PI / .75;
                b = I / 3 * (1 + S * Math.Cos(H) / Math.Cos(Math.PI / 3 - H));
                r = I / 3 * (1 + S * (1 - Math.Cos(H) / Math.Cos(Math.PI / 3 - H)));
                g = I / 3 * (1 - S);
            }

            color = new LedColor((float)r, (float)g, (float)b, 0);
        }

        // https://blog.saikoled.com/post/44677718712/how-to-convert-from-hsi-to-rgb-white
        public static void HsiToRgbw(double H, double S, double I, out LedColor color)
        {
            double r, g, b, w;
            double cos_h, cos_1047_h;
            H %= 360; // cycle H around to 0-360 degrees
            H = Math.PI * H / 180.0; // Convert to radians.
            S = S > 0 ? (S < 1 ? S : 1) : 0; // clamp S and I to interval [0,1]
            I = I > 0 ? (I < 1 ? I : 1) : 0;

            if (H < Math.PI / 1.5)
            {
                cos_h = Math.Cos(H);
                cos_1047_h = Math.Cos(Math.PI / 3 - H);
                r = S * I / 3 * (1 + cos_h / cos_1047_h);
                g = S * I / 3 * (1 + (1 - cos_h / cos_1047_h));
                b = 0;
                w = (1 - S) * I;
            }
            else if (H < Math.PI / .75)
            {
                H = H - Math.PI / 1.5;
                cos_h = Math.Cos(H);
                cos_1047_h = Math.Cos(Math.PI / 3 - H);
                g = S * I / 3 * (1 + cos_h / cos_1047_h);
                b = S * I / 3 * (1 + (1 - cos_h / cos_1047_h));
                r = 0;
                w = (1 - S) * I;
            }
            else
            {
                H = H - Math.PI / .75;
                cos_h = Math.Cos(H);
                cos_1047_h = Math.Cos(Math.PI / 3 - H);
                b = S * I / 3 * (1 + cos_h / cos_1047_h);
                r = S * I / 3 * (1 + (1 - cos_h / cos_1047_h));
                g = 0;
                w = (1 - S) * I;
            }

            color = new LedColor((float)r, (float)g, (float)b, (float)w);
        }

        public static void RgbwToRgb(ColorSettingModel clr, out LedColor color)
        {
            color = new LedColor((float)Math.Clamp(clr.Red + clr.White, 0, 1),
                (float)Math.Clamp(clr.Green + clr.White, 0, 1),
                (float)Math.Clamp(clr.Blue + clr.White, 0, 1),
                0);
        }

        public static void ColorToHSV(Color color, out float hue, out float saturation, out float value)
        {
            float max = Math.Max(color.Red, Math.Max(color.Green, color.Blue));
            float min = Math.Min(color.Red, Math.Min(color.Green, color.Blue));

            hue = color.GetHue() * 360;
            saturation = (max == 0) ? 0 : 1f - (min / max);
            value = max;
        }
    }
}
