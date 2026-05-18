using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Propertify.Mobile.Models;
using Propertify.Mobile.Services;
using System.Collections.ObjectModel;

namespace Propertify.Mobile.ViewModels
{
    public partial class MaintenanceViewModel : ObservableObject
    {
        private readonly ApiService     _api;
        private readonly SessionService _session;
        private List<MaintenanceDto> _allRequests = new();

        [ObservableProperty] private bool   isBusy       = false;
        [ObservableProperty] private bool   isEmpty      = false;
        [ObservableProperty] private bool   isRefreshing = false;
        [ObservableProperty] private string activeFilter = "All";

        public ObservableCollection<MaintenanceDto> Requests { get; } = new();

        public MaintenanceViewModel(ApiService api, SessionService session)
        {
            _api     = api;
            _session = session;
        }

        public async Task LoadAsync()
        {
            IsBusy      = true;
            _allRequests = await _api.GetMaintenanceAsync(_session.UnitId);
            IsBusy      = false;
            ApplyFilter(ActiveFilter);
        }

        public void ApplyFilter(string filter)
        {
            ActiveFilter = filter;
            Requests.Clear();
            var filtered = filter == "All"
                ? _allRequests
                : _allRequests.Where(r => r.Status.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var r in filtered) Requests.Add(r);
            IsEmpty = Requests.Count == 0;
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
