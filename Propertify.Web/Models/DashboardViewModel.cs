namespace Propertify.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public int VacantUnits { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal PredictedNextMonthRevenue { get; set; }
        public List<MaintenanceRequest> RecentRequests { get; set; } = [];
    }
}