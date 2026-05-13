using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. عرض الصفحة الرئيسية مع العقارات الشاغرة فقط
        public async Task<IActionResult> Index()
        {
            // جلب أول 6 وحدات شاغرة لعرضها في قسم "Featured Units"
            var availableUnits = await _context.Units
                .Include(u => u.Property) // جلب بيانات المبنى المرتبط بالوحدة
                .Where(u => u.IsOccupied == false) // الشرط الأساسي: غير محجوزة
                .OrderByDescending(u => u.Id)
                .Take(6)
                .ToListAsync();

            ViewBag.AvailableUnits = availableUnits;
            return View(await _context.Properties.ToListAsync());
        }

        // 1.5. عرض جميع الوحدات الشاغرة
        public async Task<IActionResult> AllUnits()
        {
            var units = await _context.Units
                .Include(u => u.Property)
                .Where(u => !u.IsOccupied)
                .OrderByDescending(u => u.Id)
                .ToListAsync();
            return View(units);
        }

        public IActionResult About()
        {
            return View();
        }

        // 2. عرض صفحة تفاصيل العقار
        public async Task<IActionResult> Details(int id)
        {
            var unit = await _context.Units
                .Include(u => u.Property)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (unit == null)
            {
                return NotFound();
            }

            return View(unit);
        }

        // 3. صفحة استمارة الطلب (Inquiry)
        public IActionResult Inquiry(string unit)
        {
            // إذا جاء الزائر من صفحة تفاصيل وحدة معينة، سنمرر رقمها للاستمارة
            ViewBag.SelectedUnit = unit;
            return View();
        }
    }
}