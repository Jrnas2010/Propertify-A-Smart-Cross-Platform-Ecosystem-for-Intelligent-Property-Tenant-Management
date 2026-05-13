using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Models;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    public AdminController(ApplicationDbContext context) => _context = context;

    // 1. عرض الداشبورد الديناميكي
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel {
            TotalUnits = await _context.Units.CountAsync(),
            OccupiedUnits = await _context.Units.CountAsync(u => u.IsOccupied),
            VacantUnits = await _context.Units.CountAsync(u => !u.IsOccupied),
            MonthlyRevenue = await _context.Contracts.SumAsync(c => c.RentAmount),
            // ميزة الذكاء الاصطناعي (تنبؤ بسيط): الدخل المتوقع بناءً على العقود النشطة
            PredictedNextMonthRevenue = await _context.Contracts
                .Where(c => c.EndDate > DateTime.Now.AddMonths(1))
                .SumAsync(c => c.RentAmount)
        };
        return View(model);
    }

    // 2. داشبورد تحليلي متقدم
    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.Now;
        var nextMonth = today.AddMonths(1);

        var currentRevenue = await _context.Contracts.SumAsync(c => c.RentAmount);

        var predictedRevenue = await _context.Contracts
            .Where(c => c.EndDate >= nextMonth)
            .SumAsync(c => c.RentAmount);

        var occupiedCount = await _context.Units.CountAsync(u => u.IsOccupied);
        var vacantCount = await _context.Units.CountAsync(u => !u.IsOccupied);

        var riskyUnits = await _context.Contracts
            .Include(c => c.Unit)
            .Where(c => c.EndDate <= nextMonth)
            .ToListAsync();

        ViewBag.CurrentRevenue = currentRevenue;
        ViewBag.PredictedRevenue = predictedRevenue;
        ViewBag.OccupiedCount = occupiedCount;
        ViewBag.VacantCount = vacantCount;
        ViewBag.RiskyUnitsCount = riskyUnits.Count;

        return View();
    }

    // 3. تصدير بيانات المستأجرين إلى Excel
    public IActionResult ExportTenants()
    {
        using (var workbook = new XLWorkbook()) {
            var worksheet = workbook.Worksheets.Add("Tenants");
            var tenants = _context.Tenants.ToList();
            worksheet.Cell(1, 1).Value = "Full Name";
            worksheet.Cell(1, 2).Value = "Phone";
            
            for (int i = 0; i < tenants.Count; i++) {
                worksheet.Cell(i + 2, 1).Value = tenants[i].FullNameEn;
                worksheet.Cell(i + 2, 2).Value = tenants[i].Phone;
            }

            using (var stream = new MemoryStream()) {
                workbook.SaveAs(stream);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TenantsReport.xlsx");
            }
        }
    }
}