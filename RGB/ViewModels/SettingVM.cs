using RGB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.ViewModels
{
    public class SettingVM
    {
        protected SettingModel settingModel;

        public SettingVM(SettingModel settingModel)
        {
            this.settingModel = settingModel;
        }

        public string Name { get => settingModel.Name; }
    }
}
