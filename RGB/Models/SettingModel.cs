using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models
{
    public class SettingModel
    {
        protected AbstractEffectModel effect;
        public string Name { get; set; }

        public SettingModel(AbstractEffectModel effect, string Name)
        {
            this.effect = effect;
            this.Name = Name;
        }
    }
}
