using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propertify.Web.Data;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class ContractController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContractController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 100)
                searchTerm = searchTerm[..100];

            ViewBag.SearchTerm = searchTerm;

            var query = _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Unit)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    (c.Tenant != null && (c.Tenant.FirstNameAr.Contains(searchTerm) ||
                                         c.Tenant.LastNameAr.Contains(searchTerm))) ||
                    (c.Unit != null && c.Unit.UnitNumber.Contains(searchTerm)));
            }

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Tenants = await _context.Tenants.ToListAsync();
            ViewBag.Units = await _context.Units.Where(u => !u.IsOccupied).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contract);

                var unit = await _context.Units.FindAsync(contract.UnitId);
                if (unit != null)
                {
                    unit.IsOccupied = true;
                    _context.Update(unit);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tenants = await _context.Tenants.ToListAsync();
            ViewBag.Units = await _context.Units.Where(u => !u.IsOccupied).ToListAsync();
            return View(contract);
        }
    }
}
