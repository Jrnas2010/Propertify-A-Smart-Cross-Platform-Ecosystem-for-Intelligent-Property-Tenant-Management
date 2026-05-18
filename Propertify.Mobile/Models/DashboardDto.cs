namespace Propertify.Mobile.Models
{
    public class DashboardDto
    {
        public string TenantName              { get; set; } = string.Empty;
        public string UnitNumber              { get; set; } = string.Empty;
        public string PropertyName            { get; set; } = string.Empty;
        public string ContractStart           { get; set; } = string.Empty;
        public string ContractEnd             { get; set; } = string.Empty;
        public int    ContractDaysRemaining   { get; set; }
        public double ContractProgress        { get; set; }
        public string ContractStatus          { get; set; } = string.Empty;
        public decimal MonthlyRent            { get; set; }
        public int    PendingBills            { get; set; }
        public decimal UnpaidAmount           { get; set; }
        public int    PendingMaintenance      { get; set; }
        public List<InvoiceDto>      RecentBills       { get; set; } = new();
        public List<MaintenanceDto>  RecentMaintenance { get; set; } = new();
    }
}
