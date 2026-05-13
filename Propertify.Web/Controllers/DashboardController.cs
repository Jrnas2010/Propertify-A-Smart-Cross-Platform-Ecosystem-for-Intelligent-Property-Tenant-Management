using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Models;
using System.Globalization;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. إحصائيات الوحدات
            var units = await _context.Units.ToListAsync();
            var totalUnits = units.Count;
            var occupiedCount = units.Count(u => u.IsOccupied);
            var vacantCount = units.Count(u => !u.IsOccupied);
            var maintenanceUnitCount = units.Count(u => u.Status == "Maintenance");

            ViewBag.OccupiedCount = occupiedCount;
            ViewBag.VacantCount = vacantCount;
            ViewBag.VacantUnits = vacantCount;
            ViewBag.MaintenanceCount = await _context.MaintenanceRequests.CountAsync(r => r.Status == "InProgress");

            // نسب مئوية لمخطط Occupancy
            ViewBag.OccupiedPct = totalUnits > 0 ? (int)Math.Round(occupiedCount * 100.0 / totalUnits) : 0;
            ViewBag.VacantPct = totalUnits > 0 ? (int)Math.Round(vacantCount * 100.0 / totalUnits) : 0;
            ViewBag.MaintenancePct = totalUnits > 0 ? (int)Math.Round(maintenanceUnitCount * 100.0 / totalUnits) : 0;
            ViewBag.OccupancyChartData = new[] { occupiedCount, vacantCount, maintenanceUnitCount };

            // 2. إحصائيات المباني والإيرادات والمصاريف
            ViewBag.TotalBuildings = await _context.Properties.CountAsync();
            ViewBag.TotalIncome = await _context.Contracts.SumAsync(c => (decimal?)c.RentAmount) ?? 0m;
            ViewBag.TotalExpenses = await _context.MaintenanceRequests.SumAsync(r => (decimal?)r.Cost) ?? 0m;

            // 3. تحليل الدخل والمصاريف لآخر 6 أشهر
            var chartLabels = new List<string>();
            var revenueData = new List<decimal>();
            var expenseData = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                chartLabels.Add(targetDate.ToString("MMM", CultureInfo.InvariantCulture));

                var monthlyIncome = await _context.Contracts
                    .Where(c => c.StartDate.Month == targetDate.Month && c.StartDate.Year == targetDate.Year)
                    .SumAsync(c => (decimal?)c.RentAmount) ?? 0m;

                var monthlyExpense = await _context.MaintenanceRequests
                    .Where(r => r.CreatedAt.Month == targetDate.Month && r.CreatedAt.Year == targetDate.Year)
                    .SumAsync(r => (decimal?)r.Cost) ?? 0m;

                revenueData.Add(monthlyIncome);
                expenseData.Add(monthlyExpense);
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.RevenueData = revenueData;
            ViewBag.ExpenseData = expenseData;

            // 4. البيانات المالية لكل عقار
            var propertyStats = await _context.Properties
                .Select(p => new {
                    p.Name,
                    TotalRevenue = _context.Contracts
                        .Where(c => c.Unit != null && c.Unit.PropertyId == p.Id)
                        .Sum(c => (decimal?)c.RentAmount) ?? 0m,
                    UnitCount = p.Units.Count
                }).ToListAsync();

            ViewBag.PropertyNames = propertyStats.Select(p => p.Name).ToList();
            ViewBag.PropertyRevenues = propertyStats.Select(p => p.TotalRevenue).ToList();

            // 5. العقود المنتهية قريباً
            var threeMonthsFromNow = DateTime.Now.AddMonths(3);
            ViewBag.ExpiringContracts = await _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Unit)
                .Where(c => c.EndDate <= threeMonthsFromNow && c.EndDate >= DateTime.Now)
                .ToListAsync();

            ViewBag.PendingMaintenance = await _context.MaintenanceRequests
                .Where(r => r.Status == "Pending")
                .ToListAsync();

            // 6. استفسارات الزوار
            ViewBag.NewInquiriesCount = await _context.BookingRequests.CountAsync(r => !r.IsRead);
            ViewBag.LatestInquiries = await _context.BookingRequests
                .OrderByDescending(r => r.SubmittedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
