using Propertify.Mobile.ViewModels;

namespace Propertify.Mobile.Views;

public partial class MaintenancePage : ContentPage
{
    private readonly MaintenanceViewModel _vm;

    public MaintenancePage(MaintenanceViewModel vm)
    {
        InitializeComponent();
        _vm            = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Requests.Count == 0)
            await _vm.LoadAsync();
    }

    private void SetActiveChip(Border active)
    {
        var chips = new[] { ChipAll, ChipPending, ChipInProgress, ChipDone };
        foreach (var chip in chips)
        {
            chip.BackgroundColor = Color.FromArgb("#FFFFFF");
            chip.StrokeThickness = 1;
            if (chip.Content is Label l) l.TextColor = Color.FromArgb("#64748b");
        }
        active.BackgroundColor = Color.FromArgb("#1e3a5f");
        active.StrokeThickness = 0;
        if (active.Content is Label lbl) lbl.TextColor = Colors.White;
    }

    private void OnFilterAll(object? sender, TappedEventArgs e)        { SetActiveChip(ChipAll);        _vm.ApplyFilter("All"); }
    private void OnFilterPending(object? sender, TappedEventArgs e)    { SetActiveChip(ChipPending);    _vm.ApplyFilter("Pending"); }
    private void OnFilterInProgress(object? sender, TappedEventArgs e) { SetActiveChip(ChipInProgress); _vm.ApplyFilter("InProgress"); }
    private void OnFilterDone(object? sender, TappedEventArgs e)       { SetActiveChip(ChipDone);       _vm.ApplyFilter("Done"); }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("addmaintenance");
    }
}
