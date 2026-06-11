using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                    .ThenInclude(u => u!.Property)
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
            var tenants = await _context.Tenants.ToListAsync();
            var units = await _context.Units.Where(u => !u.IsOccupied).ToListAsync();
            
            ViewBag.TenantId = new SelectList(tenants, "Id", "FullNameAr");
            ViewBag.UnitId = new SelectList(units, "Id", "UnitNumber");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (contract.EndDate <= contract.StartDate)
                ModelState.AddModelError(nameof(contract.EndDate), "End date must be after start date.");

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

            ViewBag.TenantId = new SelectList(await _context.Tenants.ToListAsync(), "Id", "FullNameAr", contract.TenantId);
            ViewBag.UnitId = new SelectList(await _context.Units.Where(u => !u.IsOccupied).ToListAsync(), "Id", "UnitNumber", contract.UnitId);
            return View(contract);
        }

        public async Task<IActionResult> Details(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Tenant)
                .Include(c => c.Unit)
                    .ThenInclude(u => u.Property)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null) return NotFound();

            return View(contract);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            var tenants = await _context.Tenants.ToListAsync();
            var units = await _context.Units.Where(u => !u.IsOccupied || u.Id == contract.UnitId).ToListAsync();

            ViewBag.TenantId = new SelectList(tenants, "Id", "FullNameAr", contract.TenantId);
            ViewBag.UnitId = new SelectList(units, "Id", "UnitNumber", contract.UnitId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.Id) return NotFound();

            if (contract.EndDate <= contract.StartDate)
                ModelState.AddModelError(nameof(contract.EndDate), "End date must be after start date.");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingContract = await _context.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingContract == null) return NotFound();

                    // Handle Unit Change
                    if (existingContract.UnitId != contract.UnitId)
                    {
                        var oldUnit = await _context.Units.FindAsync(existingContract.UnitId);
                        if (oldUnit != null)
                        {
                            oldUnit.IsOccupied = false;
                            oldUnit.Status = "Vacant";
                        }

                        var newUnit = await _context.Units.FindAsync(contract.UnitId);
                        if (newUnit != null)
                        {
                            newUnit.IsOccupied = true;
                            newUnit.Status = "Occupied";
                        }
                    }

                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Contracts.Any(e => e.Id == contract.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TenantId = new SelectList(await _context.Tenants.ToListAsync(), "Id", "FullNameAr", contract.TenantId);
            ViewBag.UnitId = new SelectList(await _context.Units.Where(u => !u.IsOccupied || u.Id == contract.UnitId).ToListAsync(), "Id", "UnitNumber", contract.UnitId);
            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                // Optionally free up the unit when contract is deleted
                var unit = await _context.Units.FindAsync(contract.UnitId);
                if (unit != null)
                {
                    unit.IsOccupied = false;
                    unit.Status = "Vacant";
                }

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
