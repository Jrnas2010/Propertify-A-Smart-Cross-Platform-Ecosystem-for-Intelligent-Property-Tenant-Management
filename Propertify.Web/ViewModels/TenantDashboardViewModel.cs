namespace Propertify.Web.ViewModels
{
    public class TenantDashboardViewModel
    {
        public string TenantName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public DateTime ContractExpiryDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public bool IsRentPaid { get; set; }
        public decimal LastElectricityBill { get; set; }
        public string ElectricityReading { get; set; } = string.Empty;
        public decimal LastWaterBill { get; set; }
        public string WaterReading { get; set; } = string.Empty;
        public List<BillItemViewModel> RecentBills { get; set; } = new List<BillItemViewModel>();
    }

    public class BillItemViewModel
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "Electricity", "Water", "Rent"
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
    }
}
