using Propertify.Mobile.ViewModels;

namespace Propertify.Mobile.Views;

public partial class ContractsPage : ContentPage
{
    private readonly ContractsViewModel _vm;

    public ContractsPage(ContractsViewModel vm)
    {
        InitializeComponent();
        _vm            = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Contracts.Count == 0)
            await _vm.LoadAsync();
    }
}
