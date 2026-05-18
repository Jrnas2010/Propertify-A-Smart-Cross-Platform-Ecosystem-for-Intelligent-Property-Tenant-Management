using Propertify.Mobile.ViewModels;

namespace Propertify.Mobile.Views;

public partial class MainPage : ContentPage
{
    private readonly DashboardViewModel _vm;

    public MainPage(DashboardViewModel vm)
    {
        InitializeComponent();
        _vm            = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_vm.HasData)
            await _vm.LoadAsync();
    }
}
