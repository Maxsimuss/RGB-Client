using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;

namespace RGB.Util;

public partial class CustomButton : ContentView
{
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(RelayCommand), typeof(CustomButton), null);
    public static readonly BindableProperty NameProperty = BindableProperty.Create(nameof(Name), typeof(string), typeof(CustomButton), null);
    
    public RelayCommand Command
    {
        get => (RelayCommand)GetValue(CommandProperty);
        set
        {
            SetValue(CommandProperty, value);
            OnPropertyChanged("Command");
        }
    }

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set
        {
            SetValue(NameProperty, value);
            OnPropertyChanged("Name");
        }
    }

    public CustomButton()
    {
        InitializeComponent();

        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
        GestureRecognizers.Add(tapGestureRecognizer);
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Command.Execute(null);
    }
}