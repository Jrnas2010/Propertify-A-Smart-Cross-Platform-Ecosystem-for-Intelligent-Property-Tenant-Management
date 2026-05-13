using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [AllowAnonymous] // للسماح للزوار بالوصول للرابط
        public async Task<IActionResult> SubmitInquiry(BookingRequest request)
        {
            if (ModelState.IsValid)
            {
                _context.BookingRequests.Add(request);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Request sent successfully!" });
            }
            return BadRequest();
        }
    }
}