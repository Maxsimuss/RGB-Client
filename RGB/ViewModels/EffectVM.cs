using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RGB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB.ViewModels
{
    [INotifyPropertyChanged]
    public partial class EffectVM
    {
        public AbstractEffectModel Effect;
        private AppVM appVM;

        public EffectVM(AbstractEffectModel effect, AppVM appVM)
        {
            this.Effect = effect;
            this.appVM = appVM;
        }

        public string Name { get => Effect.Name; }
        public List<SettingVM> Settings
        {
            get => Effect.Settings.Select((s) =>
            {
                if (s is SingleValueSettingModel)
                    return (SettingVM)new SingleValueSettingVM((SingleValueSettingModel)s);

                return (SettingVM)new ColorSettingVM((ColorSettingModel)s);
            }).ToList();
        }

        private bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        [RelayCommand]
        public void SelectCurrent()
        {
            appVM.CurrentEffect.Selected = false;
            appVM.CurrentEffect = this;
            Selected = true;
        }

        public Color Color
        {
            get => Selected ? Color.FromRgba("#60CDFFFF") : Color.FromRgba("#60CDFF00");
        }
    }
}
