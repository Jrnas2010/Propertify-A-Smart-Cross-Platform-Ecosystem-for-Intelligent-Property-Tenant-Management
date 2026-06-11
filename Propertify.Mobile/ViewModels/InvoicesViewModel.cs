using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Propertify.Mobile.Models;
using Propertify.Mobile.Services;
using System.Collections.ObjectModel;

namespace Propertify.Mobile.ViewModels
{
    public partial class InvoicesViewModel : ObservableObject
    {
        private readonly ApiService     _api;
        private readonly SessionService _session;
        private List<InvoiceDto> _allInvoices = new();

        [ObservableProperty] public partial bool   IsBusy       { get; set; } = false;
        [ObservableProperty] public partial bool   IsEmpty      { get; set; } = false;
        [ObservableProperty] public partial bool   IsRefreshing { get; set; } = false;
        [ObservableProperty] public partial string ActiveFilter { get; set; } = "All";

        public ObservableCollection<InvoiceDto> Invoices { get; } = new();

        public InvoicesViewModel(ApiService api, SessionService session)
        {
            _api     = api;
            _session = session;
        }

        public async Task LoadAsync()
        {
            IsBusy       = true;
            _allInvoices = await _api.GetInvoicesAsync(_session.TenantId);
            IsBusy       = false;
            ApplyFilter(ActiveFilter);
        }

        public void ApplyFilter(string filter)
        {
            ActiveFilter = filter;
            Invoices.Clear();
            var filtered = filter == "All"
                ? _allInvoices
                : _allInvoices.Where(i => i.Status.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var inv in filtered) Invoices.Add(inv);
            IsEmpty = Invoices.Count == 0;
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadAsync();
            IsRefreshing = false;
        }
    }
}
