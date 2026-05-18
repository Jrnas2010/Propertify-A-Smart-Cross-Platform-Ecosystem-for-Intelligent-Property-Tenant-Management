using Propertify.Mobile.ViewModels;

namespace Propertify.Mobile.Views;

public partial class InvoicesPage : ContentPage
{
    private readonly InvoicesViewModel _vm;

    public InvoicesPage(InvoicesViewModel vm)
    {
        InitializeComponent();
        _vm            = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Invoices.Count == 0)
            await _vm.LoadAsync();
    }

    private void SetActiveChip(Border active)
    {
        var chips = new[] { ChipAll, ChipUnpaid, ChipPaid, ChipOverdue };
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

    private void OnFilterAll(object sender, TappedEventArgs e)    { SetActiveChip(ChipAll);    _vm.ApplyFilter("All"); }
    private void OnFilterUnpaid(object sender, TappedEventArgs e) { SetActiveChip(ChipUnpaid); _vm.ApplyFilter("Unpaid"); }
    private void OnFilterPaid(object sender, TappedEventArgs e)   { SetActiveChip(ChipPaid);   _vm.ApplyFilter("Paid"); }
    private void OnFilterOverdue(object sender, TappedEventArgs e){ SetActiveChip(ChipOverdue);_vm.ApplyFilter("Overdue"); }
}
