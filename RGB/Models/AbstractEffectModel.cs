using RGB.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.Models
{
    public abstract class AbstractEffectModel
    {
        public string Name { get; protected set; }

        public bool IsDirty = false;

        public List<SettingModel> Settings = new List<SettingModel>();

        public abstract void GetColors(LedColor[] colors, bool hasWhite);
        public virtual void Begin() { }
        public virtual void End() { }
    }
}
