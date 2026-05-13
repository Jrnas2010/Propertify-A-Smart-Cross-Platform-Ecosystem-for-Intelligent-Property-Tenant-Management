using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Propertify.Web.Data;
using Propertify.Web.Models;

namespace Propertify.Web.Controllers
{
    [Authorize(Roles = "Owner")] // The owner can access this controller to manage communications
    public class CommunicationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommunicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.SentMessages = await _context.SystemMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string target, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Target and message are required.";
                return RedirectToAction(nameof(Index));
            }

            _context.SystemMessages.Add(new SystemMessage
            {
                Subject = subject?.Trim(),
                Content = message.Trim(),
                Target = target.Trim(),
                IsBroadcast = target == "all",
                SentAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Message has been sent to {target}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var msg = await _context.SystemMessages.FindAsync(id);
            if (msg != null)
            {
                _context.SystemMessages.Remove(msg);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}