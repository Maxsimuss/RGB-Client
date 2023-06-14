namespace RGB.Views;

public partial class TimerPage : ContentPage
{
	public TimerPage()
	{
		InitializeComponent();
        BindingContext = AppShell.AppVMInstance;
    }
}