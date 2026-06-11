using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Propertify.Mobile.Models;
using Propertify.Mobile.Services;
using System.Collections.ObjectModel;

namespace Propertify.Mobile.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService     _api;
        private readonly SessionService _session;

        [ObservableProperty] public partial bool   IsBusy               { get; set; } = false;
        [ObservableProperty] public partial bool   HasData              { get; set; } = false;
        [ObservableProperty] public partial string Greeting             { get; set; } = string.Empty;
        [ObservableProperty] public partial string TenantName           { get; set; } = string.Empty;
        [ObservableProperty] public partial string UnitInfo             { get; set; } = string.Empty;
        [ObservableProperty] public partial string ContractStart        { get; set; } = string.Empty;
        [ObservableProperty] public partial string ContractEnd          { get; set; } = string.Empty;
        [ObservableProperty] public partial string ContractStatus       { get; set; } = string.Empty;
        [ObservableProperty] public partial string ContractStatusColor  { get; set; } = "#10b981";
        [ObservableProperty] public partial int    DaysRemaining        { get; set; } = 0;
        [ObservableProperty] public partial double ContractProgress     { get; set; } = 0;
        [ObservableProperty] public partial string MonthlyRent          { get; set; } = string.Empty;
        [ObservableProperty] public partial int    PendingBills         { get; set; } = 0;
        [ObservableProperty] public partial string UnpaidAmount         { get; set; } = string.Empty;
        [ObservableProperty] public partial int    PendingMaintenance   { get; set; } = 0;

        public ObservableCollection<InvoiceDto>     RecentBills       { get; } = new();
        public ObservableCollection<MaintenanceDto> RecentMaintenance { get; } = new();

        public DashboardViewModel(ApiService api, SessionService session)
        {
            _api     = api;
            _session = session;
        }

        public async Task LoadAsync()
        {
            IsBusy = true;
            var hour = DateTime.Now.Hour;
            Greeting = hour < 12 ? "Good Morning" : hour < 17 ? "Good Afternoon" : "Good Evening";

            var data = await _api.GetDashboardAsync(_session.TenantId);
            IsBusy = false;

            if (data == null) { HasData = false; return; }

            TenantName          = data.TenantName;
            UnitInfo            = $"Unit {data.UnitNumber} · {data.PropertyName}";
            ContractStart       = data.ContractStart;
            ContractEnd         = data.ContractEnd;
            ContractStatus      = data.ContractStatus;
            ContractStatusColor = data.ContractStatus == "Active" ? "#10b981"
                                : data.ContractStatus == "Expired" ? "#ef4444" : "#f59e0b";
            DaysRemaining       = data.ContractDaysRemaining;
            ContractProgress    = data.ContractProgress;
            MonthlyRent         = $"{data.MonthlyRent:N3} OMR";
            PendingBills        = data.PendingBills;
            UnpaidAmount        = $"{data.UnpaidAmount:N3} OMR";
            PendingMaintenance  = data.PendingMaintenance;

            RecentBills.Clear();
            foreach (var b in data.RecentBills) RecentBills.Add(b);

            RecentMaintenance.Clear();
            foreach (var m in data.RecentMaintenance) RecentMaintenance.Add(m);

            HasData = true;
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadAsync();
    }
}
