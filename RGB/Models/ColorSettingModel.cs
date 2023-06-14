using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models
{
    class ColorSettingModel : SettingModel
    {
        public ColorSettingModel(AbstractEffectModel effect, string Name) : base(effect, Name)
        {
            this.White = Preferences.Default.Get(effect.Name + "_" + Name + "_Alpha", White);
            this.Red = Preferences.Default.Get(effect.Name + "_" + Name + "_Red", Red);
            this.Green = Preferences.Default.Get(effect.Name + "_" + Name + "_Green", Green);
            this.Blue = Preferences.Default.Get(effect.Name + "_" + Name + "_Blue", Blue);
        }

        private double white;
        public double White
        {
            get => white;
            set
            {
                white = value;
                effect.IsDirty = true;
                Preferences.Default.Set(effect.Name + "_" + Name + "_Alpha", White);
            }
        }

        private double red;
        public double Red
        {
            get => red;
            set
            {
                red = value;
                effect.IsDirty = true;
                Preferences.Default.Set(effect.Name + "_" + Name + "_Red", Red);
            }
        }

        private double green;
        public double Green
        {
            get => green;
            set
            {
                green = value;
                effect.IsDirty = true;
                Preferences.Default.Set(effect.Name + "_" + Name + "_Green", Green);
            }
        }

        private double blue;
        public double Blue
        {
            get => blue;
            set
            {
                blue = value;
                effect.IsDirty = true;
                Preferences.Default.Set(effect.Name + "_" + Name + "_Blue", Blue);
            }
        }
    }
}
