using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Propertify.Mobile.Models;
using Propertify.Mobile.Services;
using System.Collections.ObjectModel;

namespace Propertify.Mobile.ViewModels
{
    public partial class ContractsViewModel : ObservableObject
    {
        private readonly ApiService     _api;
        private readonly SessionService _session;

        [ObservableProperty] public partial bool IsBusy       { get; set; } = false;
        [ObservableProperty] public partial bool IsEmpty      { get; set; } = false;
        [ObservableProperty] public partial bool IsRefreshing { get; set; } = false;

        public ObservableCollection<ContractDto> Contracts { get; } = new();

        public ContractsViewModel(ApiService api, SessionService session)
        {
            _api     = api;
            _session = session;
        }

        public async Task LoadAsync()
        {
            IsBusy = true;
            var items = await _api.GetContractsAsync(_session.TenantId);
            IsBusy = false;

            Contracts.Clear();
            foreach (var c in items) Contracts.Add(c);
            IsEmpty = Contracts.Count == 0;
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
