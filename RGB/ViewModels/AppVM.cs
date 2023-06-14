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
    public partial class AppVM
    {
        private AppModel appModel;

        public AppVM(AppModel appModel)
        {
            this.appModel = appModel;
            appModel.controller.PropertyChanged += Controller_PropertyChanged;

            Effects = appModel.effects.Select(s => new EffectVM(s, this)).ToList();

            Application.Current.Windows[0].Destroying += AppViewModel_Destroying;
            Application.Current.Windows[0].Resumed += AppViewModel_Resumed;
            CurrentEffect.Selected = true;
        }

        private void Controller_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ConnectedDevicesMsg));
        }

        private void AppViewModel_Resumed(object sender, EventArgs e)
        {
            appModel.Start();
        }

        private void AppViewModel_Destroying(object sender, EventArgs e)
        {
            appModel.Stop();
        }

        [RelayCommand]
        public void Refresh()
        {
            appModel.controller.Connect();
        }

        [RelayCommand]
        public void Timer()
        {
            Shell.Current.GoToAsync("//TimerPage");
        }

        [RelayCommand]
        public void Submit()
        {
            ulong unixTimestamp = (ulong)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            unixTimestamp = (ulong)Math.Floor((double)unixTimestamp / 86400);
            unixTimestamp *= 86400;
            unixTimestamp += (ulong)pickedTime.TotalSeconds;
            unixTimestamp += (ulong)(DateTime.UtcNow - DateTime.Now).TotalSeconds;

            unixTimestamp *= 1000;

            //unixTimestamp.Hour -= pickedTime.Hours;

            appModel.controller.AnnounceTimer(timerEnabled, unixTimestamp, pickedColor.Red, pickedColor.Green, pickedColor.Blue, 0);
        }

        [ObservableProperty]
        public bool timerEnabled = true;

        [ObservableProperty]
        public Color pickedColor = Colors.White;

        [ObservableProperty]
        public TimeSpan pickedTime;

        public string ConnectedDevicesMsg
        {
            get
            {
                return appModel.controller.ClientCount + " Devices connected";
            }
        }

        public List<EffectVM> Effects { get; set; }

        public EffectVM CurrentEffect
        {
            get => Effects.Find(s => s.Effect == appModel.controller.CurrentEffect);
            set
            {
                if (value == null) return;

                Preferences.Default.Set("CurrentEffect", value.Effect.Name);

                appModel.controller.CurrentEffect = value.Effect;

                OnPropertyChanged(nameof(CurrentEffect));
            }
        }
    }
}
