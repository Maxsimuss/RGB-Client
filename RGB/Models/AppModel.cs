using RGB.Models.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models
{
    public class AppModel
    {
        public ControllerModel controller;
        public List<AbstractEffectModel> effects;

        public AppModel()
        {
            effects = new List<AbstractEffectModel>
            {
                new StaticEffect(),
                new RainbowEffect()
            };

            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                effects.Add(new BacklightEffect());
                effects.Add(new MusicEffect());
            }
            //effects.Find(s => s.Name == Preferences.Default.Get("CurrentEffect", effects[0].Name)), effects
            controller = new ControllerModel(effects.Find(s => s.Name == Preferences.Default.Get("CurrentEffect", effects[0].Name)));
            Start();
        }

        public void Start()
        {
            Task.Run(controller.Connect);
        }

        public void Stop()
        {
            controller.Dispose();
        }
    }
}
