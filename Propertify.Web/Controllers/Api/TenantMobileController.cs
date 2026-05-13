using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Propertify.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Tenant")]
    public class TenantMobileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TenantMobileController> _logger;

        public TenantMobileController(ApplicationDbContext context, ILogger<TenantMobileController> logger)
        {
            _context = context;
            _logger = logger;
        }

      
        // Get the current tenant's contract details

        [HttpGet("my-contract")]
        public async Task<IActionResult> GetMyContract()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (!int.TryParse(userId, out int parsedUserId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(parsedUserId);
            if (user == null || user.TenantId == null)
                return NotFound("Not found or not linked to a tenant.");

            var contract = await _context.Contracts
                .Include(c => c.Unit)
                .FirstOrDefaultAsync(c => c.TenantId == user.TenantId);

            if (contract == null) return NotFound("No active contract found.");

            return Ok(contract);
        }

        // Endpoint for tenants to submit maintenance requests
        [HttpPost("request-maintenance")]
        public async Task<IActionResult> RequestMaintenance([FromBody] MaintenanceRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verify the request belongs to the authenticated tenant
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int parsedUserId)) return Unauthorized();

            var user = await _context.Users.FindAsync(parsedUserId);
            if (user == null || user.TenantId == null) return Unauthorized();

            var tenant = await _context.Tenants.FindAsync(user.TenantId);
            if (tenant == null || tenant.UnitId != request.UnitId)
                return Forbid();

            try
            {
                _context.MaintenanceRequests.Add(request);
                await _context.SaveChangesAsync();
                return Ok(new { message = "The maintenance request has been submitted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save maintenance request for UnitId={UnitId}", request.UnitId);
                return StatusCode(500, new { message = "An error occurred. Please try again later." });
            }
        }
    }
}
