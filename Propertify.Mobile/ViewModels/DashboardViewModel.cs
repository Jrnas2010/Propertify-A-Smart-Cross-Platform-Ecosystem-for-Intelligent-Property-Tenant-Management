using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Propertify.Mobile.Models;
using Propertify.Mobile.Services;
using System.Collections.ObjectModel;

namespace Propertify.Mobile.ViewModels
{
    /// <summary>Provides all data for the tenant dashboard tab: contract progress, financials, and recent activity lists.</summary>
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService     _api;
        private readonly SessionService _session;

        [ObservableProperty] private bool    isBusy        = false;
        [ObservableProperty] private bool    hasData       = false;
        [ObservableProperty] private string  greeting      = string.Empty;
        [ObservableProperty] private string  tenantName    = string.Empty;
        [ObservableProperty] private string  unitInfo      = string.Empty;
        [ObservableProperty] private string  contractStart = string.Empty;
        [ObservableProperty] private string  contractEnd   = string.Empty;
        [ObservableProperty] private string  contractStatus = string.Empty;
        [ObservableProperty] private string  contractStatusColor = "#10b981";
        [ObservableProperty] private int     daysRemaining = 0;
        [ObservableProperty] private double  contractProgress = 0;
        [ObservableProperty] private string  monthlyRent   = string.Empty;
        [ObservableProperty] private int     pendingBills  = 0;
        [ObservableProperty] private string  unpaidAmount  = string.Empty;
        [ObservableProperty] private int     pendingMaintenance = 0;

        public ObservableCollection<InvoiceDto>     RecentBills       { get; } = new();
        public ObservableCollection<MaintenanceDto> RecentMaintenance { get; } = new();

        public DashboardViewModel(ApiService api, SessionService session)
        {
            _api     = api;
            _session = session;
        }

        /// <summary>Fetches dashboard data from the API and populates all observable properties and collections.</summary>
        public async Task LoadAsync()
        {
            IsBusy = true;
            var hour = DateTime.Now.Hour;
            Greeting = hour < 12 ? "Good Morning" : hour < 17 ? "Good Afternoon" : "Good Evening";

            var data = await _api.GetDashboardAsync(_session.TenantId);
            IsBusy = false;

            if (data == null) { HasData = false; return; }

            TenantName       = data.TenantName;
            UnitInfo         = $"Unit {data.UnitNumber} · {data.PropertyName}";
            ContractStart    = data.ContractStart;
            ContractEnd      = data.ContractEnd;
            ContractStatus   = data.ContractStatus;
            ContractStatusColor = data.ContractStatus == "Active" ? "#10b981"
                                : data.ContractStatus == "Expired" ? "#ef4444" : "#f59e0b";
            DaysRemaining    = data.ContractDaysRemaining;
            ContractProgress = data.ContractProgress;
            MonthlyRent      = $"{data.MonthlyRent:N3} OMR";
            PendingBills     = data.PendingBills;
            UnpaidAmount     = $"{data.UnpaidAmount:N3} OMR";
            PendingMaintenance = data.PendingMaintenance;

            RecentBills.Clear();
            foreach (var b in data.RecentBills) RecentBills.Add(b);

            RecentMaintenance.Clear();
            foreach (var m in data.RecentMaintenance) RecentMaintenance.Add(m);

            HasData = true;
        }

        /// <summary>Pull-to-refresh command – reloads dashboard data.</summary>
        [RelayCommand]
        private async Task RefreshAsync() => await LoadAsync();
    }
}
