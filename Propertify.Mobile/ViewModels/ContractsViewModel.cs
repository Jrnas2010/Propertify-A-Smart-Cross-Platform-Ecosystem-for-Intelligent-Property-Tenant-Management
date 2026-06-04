using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Propertify.Mobile.Models;
using Propertify.Mobile.Services;
using System.Collections.ObjectModel;

namespace Propertify.Mobile.ViewModels
{
    /// <summary>Loads and exposes the tenant's contracts list with pull-to-refresh support.</summary>
    public partial class ContractsViewModel : ObservableObject
    {
        private readonly ApiService     _api;
        private readonly SessionService _session;

        [ObservableProperty] private bool   isBusy    = false;
        [ObservableProperty] private bool   isEmpty   = false;
        [ObservableProperty] private bool   isRefreshing = false;

        public ObservableCollection<ContractDto> Contracts { get; } = new();

        public ContractsViewModel(ApiService api, SessionService session)
        {
            _api     = api;
            _session = session;
        }

        /// <summary>Fetches the tenant's contracts from the API and replaces the collection.</summary>
        public async Task LoadAsync()
        {
            IsBusy = true;
            var items = await _api.GetContractsAsync(_session.TenantId);
            IsBusy = false;

            Contracts.Clear();
            foreach (var c in items) Contracts.Add(c);
            IsEmpty = Contracts.Count == 0;
        }

        /// <summary>Pull-to-refresh command – sets IsRefreshing while reloading.</summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadAsync();
            IsRefreshing = false;
        }
    }
}
