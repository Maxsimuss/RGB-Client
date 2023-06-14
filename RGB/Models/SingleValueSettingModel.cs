using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models
{
    internal class SingleValueSettingModel : SettingModel
    {
        public SingleValueSettingModel(AbstractEffectModel effect, string Name, double Min, double Max, double Value) : base(effect, Name)
        {
            this.Min = Min;
            this.Max = Max;
            this.Value = Preferences.Default.Get(effect.Name + "_" + Name, Value);
        }
        public string Name { get; set; }

        private double value;
        public double Value
        {
            get => value;
            set
            {
                this.value = Math.Clamp(value, Min, Max);
                effect.IsDirty = true;
                Preferences.Default.Set(effect.Name + "_" + Name, Value);
            }
        }

        public double Min { get; set; }

        public double Max { get; set; }
    }
}
