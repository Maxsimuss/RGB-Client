using RGB.Models;
using RGB.ViewModels;

namespace RGB;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = AppShell.AppVMInstance;
    }
}

