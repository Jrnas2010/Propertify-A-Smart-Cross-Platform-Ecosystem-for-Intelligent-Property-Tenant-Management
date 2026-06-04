namespace Propertify.Mobile;

public partial class App : Application
{
    public App(IServiceProvider services)
    {
        InitializeComponent();          // App.xaml resources load here
        MainPage = services.GetRequiredService<Views.LoginPage>();
    }
}
