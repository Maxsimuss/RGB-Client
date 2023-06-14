using RGB.Models;
using RGB.ViewModels;

namespace RGB;

public partial class AppShell : Shell
{
    //BAD PRACTICES AHEAD

    public static AppVM appVM;
    public static AppVM AppVMInstance
    {
        get
        {
            if (appVM == null)
            {
                appVM = new AppVM(new AppModel());
            }

            return appVM;
        }
        set
        {
            appVM = value;
        }
    }

    public AppShell()
    {
        InitializeComponent();
    }
}
