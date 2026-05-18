using Propertify.Mobile.Views;

namespace Propertify.Mobile;

public partial class App : Application
{
    // LoginPage is resolved from DI (it receives LoginViewModel via its constructor).
    public App(LoginPage loginPage)
    {
        InitializeComponent();
        MainPage = loginPage;
    }
}
