using CommunityToolkit.Mvvm.ComponentModel;
using RGB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RGB.ViewModels
{
    [INotifyPropertyChanged]
    internal partial class ColorSettingVM : SettingVM
    {
        public ColorSettingVM(ColorSettingModel settingModel) : base(settingModel) { }

        public Color Color
        {
            get => new Color((float)Red, (float)Green, (float)Blue, (float)White);
            set
            {
                White = 0;
                Red = value.Red;
                Green = value.Green;
                Blue = value.Blue;
                OnPropertyChanged();
            }
        }

        public double White
        {
            get => ((ColorSettingModel)settingModel).White;
            set => ((ColorSettingModel)settingModel).White = value;
        }

        public double Red { get => ((ColorSettingModel)settingModel).Red; set => ((ColorSettingModel)settingModel).Red = value; }

        public double Green { get => ((ColorSettingModel)settingModel).Green; set => ((ColorSettingModel)settingModel).Green = value; }

        public double Blue { get => ((ColorSettingModel)settingModel).Blue; set => ((ColorSettingModel)settingModel).Blue = value; }
    }
}
