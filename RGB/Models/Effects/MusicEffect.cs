using CSCore.DSP;
using NAudio.Wave;
using RGB.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models.Effects
{
    internal class MusicEffect : AbstractEffectModel
    {
        WasapiLoopbackCapture wasapi = new WasapiLoopbackCapture();
        SingleValueSettingModel sensitivity, brightness;

        float amt = 0;
        float hue = 0;
        public MusicEffect()
        {
            Name = "Music";
            IsDirty = true;

            Settings.Add(sensitivity = new SingleValueSettingModel(this, "Sensitivity", 0.1, 2, 1));
            Settings.Add(brightness = new SingleValueSettingModel(this, "Brightness", 0.0, 1, 1));

            int ch = wasapi.WaveFormat.Channels;
            float avg = 0;
            LowpassFilter filter = new LowpassFilter(wasapi.WaveFormat.SampleRate, 200);
            //HighpassFilter filter2 = new HighpassFilter(wasapi.WaveFormat.SampleRate, 20);
            wasapi.DataAvailable += (s, e) =>
            {
                float[] decoded = new float[e.Buffer.Length / 4 / ch];
                float val = 0;
                for (int i = 0; i < e.Buffer.Length / 4 / ch; i++)
                {
                    float v = 0;
                    for (int j = 0; j < ch; j++)
                    {
                        v += BitConverter.ToSingle(e.Buffer, (i + j) * 4);
                    }
                    decoded[i] = v / ch;
                }
                filter.Process(decoded);
                //filter2.Process(decoded);
                for (int i = 0; i < decoded.Length; i++)
                {
                    val += Math.Abs(decoded[i]);
                }

                val /= decoded.Length;
                val *= 50;
                val *= (float)sensitivity.Value;
                avg /= 2;
                avg += val * val * val;
                avg /= 2;
                amt = avg;
            };
        }

        public override void Begin()
        {
            try
            {
                wasapi.StartRecording();
            }
            catch (InvalidOperationException) {
                wasapi.StopRecording();
                while (wasapi.CaptureState != NAudio.CoreAudioApi.CaptureState.Stopped) { Thread.Sleep(100); }

                wasapi.StartRecording();
            }
        }

        public override void End()
        {
            wasapi.StopRecording();
        }

        public override void GetColors(LedColor[] colors, bool hasWhite)
        {
            float r;
            float g;
            float b;
            hue += Math.Min(amt * amt * 30, 180);
            if(hasWhite)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    ColorUtil.HsiToRgbw((hue + i) % 360, 1 - Math.Clamp(amt - 1, 0, 1), brightness.Value * Math.Min(amt, 1), out colors[i]);
                }
            }
            else
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    ColorUtil.HsiToRgb((hue + i) % 360, 1 - Math.Clamp(amt - 1, 0, 1), brightness.Value * Math.Min(amt, 1), out colors[i]);
                }
            }
        }
    }
}
