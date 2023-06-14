using CommunityToolkit.Mvvm.ComponentModel;
using RGB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class SingleValueSettingVM : SettingVM
    {
        public SingleValueSettingVM(SingleValueSettingModel settingModel) : base(settingModel) { }

        public double Value
        {
            get => ((SingleValueSettingModel)settingModel).Value;
            set => ((SingleValueSettingModel)settingModel).Value = value;
        }
        public double Min { get => ((SingleValueSettingModel)settingModel).Min; }
        public double Max { get => ((SingleValueSettingModel)settingModel).Max; }
    }
}
