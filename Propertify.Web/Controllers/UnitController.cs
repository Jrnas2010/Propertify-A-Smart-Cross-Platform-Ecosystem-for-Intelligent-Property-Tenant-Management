using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class UnitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnitController(ApplicationDbContext context) => _context = context;

        public IActionResult CreateMultiple(int propertyId)
        {
            ViewBag.PropertyId = propertyId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiple(Unit baseUnit, List<Unit> Units, int UnitCount)
        {
            if (UnitCount > 0)
            {
                for (int i = 0; i < UnitCount; i++)
                {
                    var newUnit = new Unit
                    {
                        PropertyId = baseUnit.PropertyId,
                        FloorNumber = baseUnit.FloorNumber,
                        Area = baseUnit.Area,
                        RentAmount = baseUnit.RentAmount,
                        Bedrooms = baseUnit.Bedrooms,
                        Bathrooms = baseUnit.Bathrooms,
                        Kitchens = baseUnit.Kitchens,
                        LivingRooms = baseUnit.LivingRooms,
                        Majlis = baseUnit.Majlis,
                        UnitNumber = Units[i].UnitNumber,
                        ElectricityMeter = Units[i].ElectricityMeter,
                        WaterMeter = Units[i].WaterMeter,
                        Status = "Vacant",
                        IsOccupied = false
                    };
                    _context.Units.Add(newUnit);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Property", new { id = baseUnit.PropertyId });
            }

            return View(baseUnit);
        }
    }
}
