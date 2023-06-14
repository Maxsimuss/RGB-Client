using RGB.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models.Effects
{
    internal class BacklightEffect : AbstractEffectModel
    {
        private DesktopDuplication desktopDuplicator;
        private SingleValueSettingModel brightness, smoothing;

        [ThreadStatic]
        private static float[] avgR, avgG, avgB;

        [ThreadStatic]
        private static int[] count;

        public BacklightEffect()
        {
            Name = "Backlight";
            Settings.Add(brightness = new SingleValueSettingModel(this, "Brightness", 0, 1, 1));
            Settings.Add(smoothing = new SingleValueSettingModel(this, "Smoothing", 0, 20, 3));
            IsDirty = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Callsite not reachable on unsupported platforms.")]
        public override void Begin()
        {
            desktopDuplicator = new DesktopDuplication();
        }

        public override void End()
        {
            desktopDuplicator.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Callsite not reachable on unsupported platforms.")]
        public override void GetColors(LedColor[] colors, bool hasWhite)
        {
            int w;
            lock (desktopDuplicator)
            {
                int result = desktopDuplicator.UpdateFrame();

                if (result < 0)
                {
                    desktopDuplicator.Dispose();
                    desktopDuplicator = new DesktopDuplication();
                    result = desktopDuplicator.UpdateFrame();
                }

                w = desktopDuplicator.bitmap.Width;

                if (avgR == null)
                {
                    avgR = new float[desktopDuplicator.bitmap.Width];
                    avgG = new float[desktopDuplicator.bitmap.Width];
                    avgB = new float[desktopDuplicator.bitmap.Width];
                }

                if (result > 0)
                {
                    int smooth = (int)smoothing.Value;

                    int h = desktopDuplicator.bitmap.Height;

                    for (int i = 0; i < w; i++)
                    {
                        float r = 0, g = 0, b = 0;
                        for (int j = 0; j < h; j++)
                        {
                            System.Drawing.Color pixel = desktopDuplicator.bitmap.GetPixel(i, j);
                            r += pixel.R;
                            g += pixel.G;
                            b += pixel.B;
                        }
                        r /= h / (float)brightness.Value * 255;
                        g /= h / (float)brightness.Value * 255;
                        b /= h / (float)brightness.Value * 255;

                        avgR[i] = (r + avgR[i] * smooth) / (smooth + 1);
                        avgG[i] = (g + avgG[i] * smooth) / (smooth + 1);
                        avgB[i] = (b + avgB[i] * smooth) / (smooth + 1);
                    }
                }
            }

            if (count == null) count = new int[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new LedColor(0, 0, 0, 0);
            }

            for (int i = 0; i < w; i++)
            {
                int idx = colors.Length - 1 - i * colors.Length / w;
                colors[idx] += new LedColor(avgR[i], avgG[i], avgB[i], 0);
                count[idx]++;
            }

            if (hasWhite)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    float min = Math.Min(colors[i].R, Math.Min(colors[i].G, colors[i].B));
                    colors[i].R -= min;
                    colors[i].G -= min;
                    colors[i].B -= min;
                    colors[i].W = min * .25f;
                    colors[i] /= count[i];
                    count[i] = 0;
                }
            }
            else
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] /= count[i];
                    count[i] = 0;
                }
            }
        }
    }
}
