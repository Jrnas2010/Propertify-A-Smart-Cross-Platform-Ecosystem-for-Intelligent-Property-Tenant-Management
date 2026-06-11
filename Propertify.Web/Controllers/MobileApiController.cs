using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Propertify.Web.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Propertify.Web.Controllers
{
    /// <summary>
    /// REST API consumed by the Propertify mobile app.
    /// All endpoints are unauthenticated at the HTTP level; the Login action returns the session IDs
    /// that the app uses to scope subsequent requests.
    /// </summary>
    [Route("api/mobile")]
    [ApiController]
    public class MobileApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public MobileApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Serialises <paramref name="data"/> to camel-case JSON and wraps it in a <see cref="ContentResult"/>.</summary>
        private ContentResult Json(object data) =>
            Content(JsonSerializer.Serialize(data, JsonOpts), "application/json");

        /// <summary>Appends an ActivityLog row for the given user action (Platform = "Mobile").</summary>
        private async Task LogAsync(int userId, string userName, string action, string? details = null)
        {
            _context.ActivityLogs.Add(new Propertify.Web.Models.ActivityLog
            {
                UserId   = userId,
                UserName = userName,
                Action   = action,
                Details  = details,
                Timestamp = DateTime.UtcNow,
                Platform = "Mobile"
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Authenticates a tenant user by email/password. On success returns userId, tenantId, unitId,
        /// names, and the comma-separated permissions string the app uses for feature gating.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] MobileLoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(Json(new { success = false, message = "Email and password are required." }));

            var user = await _context.Users
                .Include(u => u.Tenant)
                    .ThenInclude(t => t!.Unit)
                        .ThenInclude(u => u!.Property)
                .FirstOrDefaultAsync(u => u.Email == req.Email.Trim().ToLower());

            if (user == null || !PasswordHelper.Verify(req.Password, user.Password))
                return Content(JsonSerializer.Serialize(new { success = false, message = "Invalid email or password." }, JsonOpts), "application/json");

            if (user.Status != "Active")
                return Content(JsonSerializer.Serialize(new { success = false, message = "Your account is inactive. Please contact the administrator." }, JsonOpts), "application/json");

            if (user.TenantId == null || user.Tenant == null)
                return Content(JsonSerializer.Serialize(new { success = false, message = "No tenant profile is linked to this account." }, JsonOpts), "application/json");

            var tenant = user.Tenant;
            var perms = string.IsNullOrWhiteSpace(user.Permissions)
                ? "Contracts,Invoices,Maintenance"
                : user.Permissions;

            await LogAsync(user.Id, tenant.FullNameEn, "Login",
                $"Logged in from mobile app. Unit: {tenant.Unit?.UnitNumber}");

            return Content(JsonSerializer.Serialize(new
            {
                success    = true,
                userId     = user.Id,
                tenantId   = tenant.Id,
                unitId     = tenant.UnitId,
                unitNumber = tenant.Unit?.UnitNumber ?? "",
                propertyName = tenant.Unit?.Property?.Name ?? "",
                fullName   = tenant.FullNameEn,
                permissions = perms
            }, JsonOpts), "application/json");
        }

        /// <summary>
        /// Returns a summary for the tenant dashboard: contract progress, pending bills/maintenance counts,
        /// unpaid amount, monthly rent, and the last 3 bills and maintenance requests.
        /// </summary>
        [HttpGet("dashboard/{tenantId}")]
        public async Task<IActionResult> Dashboard(int tenantId)
        {
            var tenant = await _context.Tenants
                .Include(t => t.Unit).ThenInclude(u => u!.Property)
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null) return NotFound(Json(new { success = false, message = "Tenant not found." }));

            var dashUser = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
            if (dashUser != null)
                await LogAsync(dashUser.Id, tenant.FullNameEn, "Viewed Dashboard",
                    $"Unit: {tenant.Unit?.UnitNumber}, Property: {tenant.Unit?.Property?.Name}");

            var contract = await _context.Contracts
                .Where(c => c.TenantId == tenantId)
                .OrderByDescending(c => c.EndDate)
                .FirstOrDefaultAsync();

            var unitId = tenant.UnitId;

            var pendingBills = await _context.UtilityBills
                .CountAsync(b => b.TenantId == tenantId && b.Status != "Paid");

            var unpaidAmount = await _context.UtilityBills
                .Where(b => b.TenantId == tenantId && b.Status != "Paid")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0m;

            var pendingMaintenance = await _context.MaintenanceRequests
                .CountAsync(r => r.UnitId == unitId && r.Status == "Pending");

            int daysRemaining = 0;
            double contractProgress = 0;
            if (contract != null)
            {
                var totalDays    = (contract.EndDate - contract.StartDate).TotalDays;
                var elapsedDays  = (DateTime.Now - contract.StartDate).TotalDays;
                daysRemaining    = Math.Max(0, (int)(contract.EndDate - DateTime.Now).TotalDays);
                contractProgress = totalDays > 0 ? Math.Clamp(elapsedDays / totalDays, 0.0, 1.0) : 0;
            }

            // Recent bills (last 3)
            var recentBills = await _context.UtilityBills
                .Where(b => b.TenantId == tenantId)
                .OrderByDescending(b => b.IssueDate)
                .Take(3)
                .Select(b => new { b.BillId, b.ServiceType, b.TotalAmount, issueDate = b.IssueDate.ToString("MMM d, yyyy"), b.Status })
                .ToListAsync();

            // Recent maintenance (last 3)
            var recentMaintenance = await _context.MaintenanceRequests
                .Where(r => r.UnitId == unitId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .Select(r => new { r.Id, r.Title, r.Status, r.Priority, createdAt = r.CreatedAt.ToString("MMM d, yyyy") })
                .ToListAsync();

            return Content(JsonSerializer.Serialize(new
            {
                tenantName   = tenant.FullNameEn,
                unitNumber   = tenant.Unit?.UnitNumber ?? "",
                propertyName = tenant.Unit?.Property?.Name ?? "",
                contractStart        = contract?.StartDate.ToString("MMM d, yyyy") ?? "",
                contractEnd          = contract?.EndDate.ToString("MMM d, yyyy") ?? "",
                contractDaysRemaining = daysRemaining,
                contractProgress,
                contractStatus  = contract?.Status ?? "None",
                monthlyRent     = contract?.MonthlyRent ?? 0m,
                pendingBills,
                unpaidAmount,
                pendingMaintenance,
                recentBills,
                recentMaintenance
            }, JsonOpts), "application/json");
        }

        /// <summary>Returns all contracts for the given tenant, ordered newest-first, with days-remaining computed.</summary>
        [HttpGet("contracts/{tenantId}")]
        public async Task<IActionResult> Contracts(int tenantId)
        {
            var contractUser = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
            var contractTenant = await _context.Tenants.FindAsync(tenantId);
            if (contractUser != null && contractTenant != null)
                await LogAsync(contractUser.Id, contractTenant.FullNameEn, "Viewed Contracts",
                    $"TenantId: {tenantId}");

            var contracts = await _context.Contracts
                .Include(c => c.Unit).ThenInclude(u => u!.Property)
                .Where(c => c.TenantId == tenantId)
                .OrderByDescending(c => c.EndDate)
                .Select(c => new
                {
                    c.Id,
                    startDate    = c.StartDate.ToString("MMM d, yyyy"),
                    endDate      = c.EndDate.ToString("MMM d, yyyy"),
                    c.RentAmount,
                    c.MonthlyRent,
                    c.Status,
                    unitNumber   = c.Unit!.UnitNumber,
                    propertyName = c.Unit.Property!.Name,
                    daysRemaining = Math.Max(0, (int)(c.EndDate - DateTime.Now).TotalDays)
                })
                .ToListAsync();

            return Content(JsonSerializer.Serialize(contracts, JsonOpts), "application/json");
        }

        /// <summary>Returns all utility bills for the given tenant, ordered newest-first.</summary>
        [HttpGet("invoices/{tenantId}")]
        public async Task<IActionResult> Invoices(int tenantId)
        {
            var invoiceUser = await _context.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
            var invoiceTenant = await _context.Tenants.FindAsync(tenantId);
            if (invoiceUser != null && invoiceTenant != null)
                await LogAsync(invoiceUser.Id, invoiceTenant.FullNameEn, "Viewed Invoices",
                    $"TenantId: {tenantId}");

            var bills = await _context.UtilityBills
                .Where(b => b.TenantId == tenantId)
                .OrderByDescending(b => b.IssueDate)
                .Select(b => new
                {
                    b.BillId,
                    b.ServiceType,
                    b.TotalAmount,
                    issueDate = b.IssueDate.ToString("MMM d, yyyy"),
                    b.Status
                })
                .ToListAsync();

            return Content(JsonSerializer.Serialize(bills, JsonOpts), "application/json");
        }

        /// <summary>Returns all maintenance requests for the given unit, ordered newest-first.</summary>
        [HttpGet("maintenance/{unitId}")]
        public async Task<IActionResult> Maintenance(int unitId)
        {
            var requests = await _context.MaintenanceRequests
                .Where(r => r.UnitId == unitId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    r.Status,
                    r.Priority,
                    createdAt = r.CreatedAt.ToString("MMM d, yyyy"),
                    r.ImagePath
                })
                .ToListAsync();

            return Content(JsonSerializer.Serialize(requests, JsonOpts), "application/json");
        }

        /// <summary>
        /// Creates a new maintenance request (Status="Pending", Priority="Normal").
        /// If an image is attached it is saved to <c>wwwroot/uploads/maintenance/</c>.
        /// </summary>
        [HttpPost("maintenance/submit")]
        public async Task<IActionResult> SubmitMaintenance([FromForm] MobileMaintenanceRequest req)

        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(Json(new { success = false, message = "Title is required." }));

            string? imagePath = null;
            if (req.ImageFile != null && req.ImageFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "maintenance");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(req.ImageFile.FileName)}";
                var filePath = Path.Combine(folder, fileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await req.ImageFile.CopyToAsync(stream);
                imagePath = $"/uploads/maintenance/{fileName}";
            }

            var unit = await _context.Units
                .Include(u => u.Property)
                .FirstOrDefaultAsync(u => u.Id == req.UnitId);

            var allowedPriorities = new HashSet<string> { "Normal", "High", "Urgent" };
            var maintenance = new Propertify.Web.Models.MaintenanceRequest
            {
                Title        = req.Title,
                Description  = req.Description ?? "",
                UnitId       = req.UnitId,
                PropertyId   = unit?.PropertyId ?? 0,
                PropertyName = unit?.Property?.Name,
                Status       = "Pending",
                Priority     = allowedPriorities.Contains(req.Priority) ? req.Priority : "Normal",
                ImagePath    = imagePath
            };

            _context.MaintenanceRequests.Add(maintenance);
            await _context.SaveChangesAsync();

            var maintUser = await _context.Users.FirstOrDefaultAsync(u => u.Tenant != null && u.Tenant.UnitId == req.UnitId);
            if (maintUser != null)
                await LogAsync(maintUser.Id, maintUser.FullName, "Submitted Maintenance Request",
                    $"Title: {req.Title}, UnitId: {req.UnitId}");

            return Content(JsonSerializer.Serialize(new { success = true, id = maintenance.Id }, JsonOpts), "application/json");
        }
    }

    public class MobileLoginRequest
    {
        public string Email    { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class MobileMaintenanceRequest
    {
        public string  Title       { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int     UnitId      { get; set; }
        public string  Priority    { get; set; } = "Normal";
        public IFormFile? ImageFile { get; set; }
    }
}
