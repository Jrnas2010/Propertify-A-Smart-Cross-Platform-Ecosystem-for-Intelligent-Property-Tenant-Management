using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Propertify.Web.Data;
using Propertify.Web.Models;
using static Propertify.Web.Models.MaintenanceStatus;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.MaintenanceRequests
                .Include(r => r.Unit)
                .Include(r => r.Property)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.PendingCount = requests.Count(r => r.Status == "Pending");
            ViewBag.InProgressCount = requests.Count(r => r.Status == "InProgress");
            ViewBag.CompletedCount = requests.Count(r => r.Status == "Completed");

            // For "Log Request" modal dropdowns
            ViewBag.Properties = await _context.Properties.ToListAsync();
            ViewBag.Units = await _context.Units.ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceRequest request)
        {
            if (ModelState.IsValid)
            {
                request.CreatedAt = DateTime.Now;
                request.Status = "Pending";

                // Auto-fill PropertyName from the linked Property
                var property = await _context.Properties.FindAsync(request.PropertyId);
                if (property != null)
                    request.PropertyName = property.Name;

                _context.MaintenanceRequests.Add(request);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, decimal cost)
        {
            if (!MaintenanceStatus.All.Contains(status)) return BadRequest();
            if (cost < 0 || cost > 999_999) return BadRequest();

            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request != null)
            {
                request.Status = status;
                request.Cost = cost;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
