using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Propertify.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Propertify.Web.Models;
using Propertify.Web.Helpers;
using Propertify.Web.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Service registration ---

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    })
    .AddViewLocalization();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "ar", "en" };
    options.SetDefaultCulture("en")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<SystemSettingService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// --- 2. Build application ---
var app = builder.Build();

// --- 3. Configure request pipeline (correct order) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed default admin user and demo data on startup
async Task SeedData(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // ── Admin user ──────────────────────────────────────────────────────────────
    if (!context.Users.Any(u => u.Role == "Owner"))
    {
        context.Users.Add(new User
        {
            Email    = "admin@propertify.com",
            Password = PasswordHelper.Hash("Admin123"),
            Role     = "Owner",
            FullName = "Admin User",
            IsSystemAdmin = true
        });
        await context.SaveChangesAsync();
    }

    // ── Guaranteed demo tenant account (runs even on an already-seeded DB) ────────
    if (!context.Users.Any(u => u.Email == "khamis@propertify.com"))
    {
        var demoUnit = await context.Units.FirstOrDefaultAsync();
        if (demoUnit != null)
        {
            var demoTenant = new Tenant
            {
                FirstNameAr  = "خميس",
                LastNameAr   = "الحمادي",
                FirstNameEn  = "Khamis",
                LastNameEn   = "Al-Hamadi",
                IdNumber     = "1009876543",
                Phone        = "+96850123456",
                Email        = "khamis@propertify.com",
                Nationality  = "Omani",
                LeaseStartDate = DateTime.Now.AddMonths(-2),
                LeaseEndDate   = DateTime.Now.AddMonths(10),
                UnitId       = demoUnit.Id
            };
            context.Tenants.Add(demoTenant);
            await context.SaveChangesAsync();

            context.Users.Add(new User
            {
                FullName    = "Khamis Al-Hamadi",
                Email       = "khamis@propertify.com",
                Password    = PasswordHelper.Hash("Khamis@1234"),
                Role        = "Tenant",
                Status      = "Active",
                Permissions = "Contracts,Invoices,Maintenance",
                TenantId    = demoTenant.Id
            });
            await context.SaveChangesAsync();
        }
    }

    // ── Demo data (only when no properties exist) ───────────────────────────────
    if (context.Properties.Any()) return;

    // 1. Buildings
    var tower = new Property
    {
        Name       = "Al-Noor Residential Tower",
        Type       = "Residential",
        Location   = "King Fahad Road, Riyadh",
        TotalUnits = 5,
        Latitude   = 24.7136,
        Longitude  = 46.6753
    };
    var heights = new Property
    {
        Name       = "Al-Salam Heights",
        Type       = "Residential",
        Location   = "Corniche Road, Jeddah",
        TotalUnits = 5,
        Latitude   = 21.5433,
        Longitude  = 39.1728
    };
    context.Properties.AddRange(tower, heights);
    await context.SaveChangesAsync();

    // 2. Units (5 per building)
    var towerUnits = new[]
    {
        new Unit { UnitNumber = "101", FloorNumber = 1, RentAmount = 3500, Area = 85,  IsOccupied = true, Status = "Occupied", Bedrooms = 2, Bathrooms = 2, Kitchens = 1, LivingRooms = 1, Majlis = 0, PropertyId = tower.Id,   ElectricityMeter = "E-101", WaterMeter = "W-101" },
        new Unit { UnitNumber = "102", FloorNumber = 1, RentAmount = 4500, Area = 110, IsOccupied = true, Status = "Occupied", Bedrooms = 3, Bathrooms = 2, Kitchens = 1, LivingRooms = 1, Majlis = 1, PropertyId = tower.Id,   ElectricityMeter = "E-102", WaterMeter = "W-102" },
        new Unit { UnitNumber = "103", FloorNumber = 2, RentAmount = 3200, Area = 80,  IsOccupied = true, Status = "Occupied", Bedrooms = 2, Bathrooms = 1, Kitchens = 1, LivingRooms = 1, Majlis = 0, PropertyId = tower.Id,   ElectricityMeter = "E-103", WaterMeter = "W-103" },
        new Unit { UnitNumber = "104", FloorNumber = 2, RentAmount = 2500, Area = 60,  IsOccupied = true, Status = "Occupied", Bedrooms = 1, Bathrooms = 1, Kitchens = 1, LivingRooms = 1, Majlis = 0, PropertyId = tower.Id,   ElectricityMeter = "E-104", WaterMeter = "W-104" },
        new Unit { UnitNumber = "105", FloorNumber = 3, RentAmount = 5500, Area = 130, IsOccupied = true, Status = "Occupied", Bedrooms = 3, Bathrooms = 3, Kitchens = 1, LivingRooms = 2, Majlis = 1, PropertyId = tower.Id,   ElectricityMeter = "E-105", WaterMeter = "W-105" },
    };
    var heightsUnits = new[]
    {
        new Unit { UnitNumber = "201", FloorNumber = 1, RentAmount = 4000, Area = 90,  IsOccupied = true, Status = "Occupied", Bedrooms = 2, Bathrooms = 2, Kitchens = 1, LivingRooms = 1, Majlis = 0, PropertyId = heights.Id, ElectricityMeter = "E-201", WaterMeter = "W-201" },
        new Unit { UnitNumber = "202", FloorNumber = 1, RentAmount = 4800, Area = 115, IsOccupied = true, Status = "Occupied", Bedrooms = 3, Bathrooms = 2, Kitchens = 1, LivingRooms = 1, Majlis = 1, PropertyId = heights.Id, ElectricityMeter = "E-202", WaterMeter = "W-202" },
        new Unit { UnitNumber = "203", FloorNumber = 2, RentAmount = 4200, Area = 95,  IsOccupied = true, Status = "Occupied", Bedrooms = 2, Bathrooms = 2, Kitchens = 1, LivingRooms = 1, Majlis = 0, PropertyId = heights.Id, ElectricityMeter = "E-203", WaterMeter = "W-203" },
        new Unit { UnitNumber = "204", FloorNumber = 2, RentAmount = 2800, Area = 65,  IsOccupied = true, Status = "Occupied", Bedrooms = 1, Bathrooms = 1, Kitchens = 1, LivingRooms = 1, Majlis = 0, PropertyId = heights.Id, ElectricityMeter = "E-204", WaterMeter = "W-204" },
        new Unit { UnitNumber = "205", FloorNumber = 3, RentAmount = 6500, Area = 150, IsOccupied = true, Status = "Occupied", Bedrooms = 4, Bathrooms = 3, Kitchens = 1, LivingRooms = 2, Majlis = 1, PropertyId = heights.Id, ElectricityMeter = "E-205", WaterMeter = "W-205" },
    };
    context.Units.AddRange(towerUnits);
    context.Units.AddRange(heightsUnits);
    await context.SaveChangesAsync();

    // 3. Tenants (one per unit, Arabic + English names)
    var allUnits = towerUnits.Concat(heightsUnits).ToArray();

    (string firstAr, string lastAr, string firstEn, string lastEn, string idNo, string phone, string email, string nat, int monthsAgo)[] tenantData =
    {
        ("أحمد",   "الراشدي",  "Ahmed",   "Al-Rashidi",  "1001234567", "+966501001001", "tenant1@propertify.com",  "Saudi",   6),
        ("محمد",   "الغامدي",  "Mohammed","Al-Ghamdi",   "1001234568", "+966501001002", "tenant2@propertify.com",  "Saudi",   5),
        ("سارة",   "العتيبي",  "Sara",    "Al-Otaibi",   "1001234569", "+966501001003", "tenant3@propertify.com",  "Saudi",   4),
        ("خالد",   "القحطاني", "Khalid",  "Al-Qahtani",  "1001234570", "+966501001004", "tenant4@propertify.com",  "Saudi",   3),
        ("فاطمة",  "الزهراني", "Fatima",  "Al-Zahrani",  "1001234571", "+966501001005", "tenant5@propertify.com",  "Saudi",   6),
        ("عمر",    "الدوسري",  "Omar",    "Al-Dossari",  "1001234572", "+966501001006", "tenant6@propertify.com",  "Saudi",   5),
        ("نور",    "الشريف",   "Nour",    "Al-Sharif",   "1001234573", "+966501001007", "tenant7@propertify.com",  "Saudi",   4),
        ("علي",    "الحربي",   "Ali",     "Al-Harbi",    "1001234574", "+966501001008", "tenant8@propertify.com",  "Saudi",   3),
        ("مريم",   "المطيري",  "Maryam",  "Al-Mutairi",  "1001234575", "+966501001009", "tenant9@propertify.com",  "Saudi",   6),
        ("يوسف",   "العنزي",   "Yousuf",  "Al-Anzi",     "1001234576", "+966501001010", "tenant10@propertify.com", "Saudi",   5),
    };

    var tenants = new List<Tenant>();
    for (int i = 0; i < tenantData.Length; i++)
    {
        var (firstAr, lastAr, firstEn, lastEn, idNo, phone, email, nat, monthsAgo) = tenantData[i];
        var start = DateTime.Now.AddMonths(-monthsAgo);
        tenants.Add(new Tenant
        {
            FirstNameAr   = firstAr,
            LastNameAr    = lastAr,
            FirstNameEn   = firstEn,
            LastNameEn    = lastEn,
            IdNumber      = idNo,
            Phone         = phone,
            Email         = email,
            Nationality   = nat,
            LeaseStartDate = start,
            LeaseEndDate   = start.AddYears(1),
            UnitId        = allUnits[i].Id
        });
    }
    context.Tenants.AddRange(tenants);
    await context.SaveChangesAsync();

    // 4. Contracts (one per tenant / unit, covering last 6 months for chart data)
    var contracts = new List<Contract>();
    for (int i = 0; i < tenants.Count; i++)
    {
        var t = tenants[i];
        var u = allUnits[i];
        contracts.Add(new Contract
        {
            TenantId    = t.Id,
            UnitId      = u.Id,
            StartDate   = t.LeaseStartDate,
            EndDate     = t.LeaseEndDate,
            RentAmount  = u.RentAmount * 12,
            MonthlyRent = u.RentAmount,
            Status      = "Active"
        });
    }
    context.Contracts.AddRange(contracts);
    await context.SaveChangesAsync();

    // 5. Tenant user accounts (mobile app login: email = tenant email, password = Tenant123)
    foreach (var t in tenants)
    {
        context.Users.Add(new User
        {
            FullName    = $"{t.FirstNameEn} {t.LastNameEn}",
            Email       = t.Email!.ToLower(),
            Password    = PasswordHelper.Hash("Tenant123"),
            Role        = "Tenant",
            Status      = "Active",
            Permissions = "Contracts,Invoices,Maintenance",
            TenantId    = t.Id
        });
    }
    await context.SaveChangesAsync();

    // 6. Suppliers
    context.Suppliers.AddRange(
        new Supplier { Name = "Al-Nour Electrical Services",  ServiceType = "Electrical",  Phone = "+966501100001", Email = "alnour@suppliers.com",      Location = "Riyadh, Industrial District",  ContactPerson = "Sami Al-Turki",    Status = "Active", CreatedAt = DateTime.Now.AddMonths(-3) },
        new Supplier { Name = "Gulf Plumbing Solutions",       ServiceType = "Plumbing",    Phone = "+966501100002", Email = "gulf@suppliers.com",         Location = "Jeddah, Al-Andalus District", ContactPerson = "Hassan Al-Sayed",  Status = "Active", CreatedAt = DateTime.Now.AddMonths(-4) },
        new Supplier { Name = "Desert Cool HVAC",              ServiceType = "HVAC",        Phone = "+966501100003", Email = "desertcool@suppliers.com",   Location = "Riyadh, Al-Malaz",            ContactPerson = "Nasser Al-Qahtani",Status = "Active", CreatedAt = DateTime.Now.AddMonths(-2) },
        new Supplier { Name = "SafeGuard Security Systems",    ServiceType = "Security",    Phone = "+966501100004", Email = "safeguard@suppliers.com",    Location = "Dammam, King Fahad Road",     ContactPerson = "Tariq Al-Hamdan",  Status = "Active", CreatedAt = DateTime.Now.AddMonths(-5) },
        new Supplier { Name = "Bright Paint & Renovation",     ServiceType = "Renovation",  Phone = "+966501100005", Email = "bright@suppliers.com",       Location = "Riyadh, Al-Ulaya",            ContactPerson = "Walid Al-Amri",    Status = "Active", CreatedAt = DateTime.Now.AddMonths(-1) }
    );
    await context.SaveChangesAsync();

    // 7. Maintenance requests (mix of statuses and priorities)
    context.MaintenanceRequests.AddRange(
        new MaintenanceRequest { Title = "AC Not Cooling",             Description = "Bedroom AC stopped cooling — needs inspection.",        Cost = 350,  Status = "Pending",    Priority = "Urgent",  PropertyId = tower.Id,   PropertyName = tower.Name,   UnitId = allUnits[0].Id, CreatedAt = DateTime.Now.AddDays(-5)  },
        new MaintenanceRequest { Title = "Water Leak Under Sink",      Description = "Dripping pipe under bathroom sink.",                    Cost = 200,  Status = "InProgress", Priority = "Normal",  PropertyId = tower.Id,   PropertyName = tower.Name,   UnitId = allUnits[1].Id, CreatedAt = DateTime.Now.AddDays(-10) },
        new MaintenanceRequest { Title = "Broken Window Latch",        Description = "Window latch broken — cannot lock properly.",           Cost = 150,  Status = "Completed",  Priority = "Normal",  PropertyId = tower.Id,   PropertyName = tower.Name,   UnitId = allUnits[2].Id, CreatedAt = DateTime.Now.AddDays(-20) },
        new MaintenanceRequest { Title = "Electrical Outlet Sparking", Description = "Kitchen outlet sparks on appliance plug-in. Urgent.",   Cost = 500,  Status = "Pending",    Priority = "Urgent",  PropertyId = heights.Id, PropertyName = heights.Name, UnitId = allUnits[5].Id, CreatedAt = DateTime.Now.AddDays(-2)  },
        new MaintenanceRequest { Title = "Elevator Unusual Noise",     Description = "Elevator emits grinding sound — needs technician.",     Cost = 1200, Status = "InProgress", Priority = "Urgent",  PropertyId = heights.Id, PropertyName = heights.Name, UnitId = allUnits[6].Id, CreatedAt = DateTime.Now.AddDays(-7)  },
        new MaintenanceRequest { Title = "Paint Touch-up Required",    Description = "Living room walls need repainting after water stain.",  Cost = 800,  Status = "Completed",  Priority = "Normal",  PropertyId = heights.Id, PropertyName = heights.Name, UnitId = allUnits[7].Id, CreatedAt = DateTime.Now.AddDays(-30) },
        new MaintenanceRequest { Title = "Door Lock Replacement",      Description = "Front door lock jammed — replacement required.",        Cost = 250,  Status = "Pending",    Priority = "Normal",  PropertyId = tower.Id,   PropertyName = tower.Name,   UnitId = allUnits[3].Id, CreatedAt = DateTime.Now.AddDays(-3)  },
        new MaintenanceRequest { Title = "Intercom System Fault",      Description = "Intercom not ringing — wiring issue suspected.",        Cost = 300,  Status = "Pending",    Priority = "Normal",  PropertyId = heights.Id, PropertyName = heights.Name, UnitId = allUnits[8].Id, CreatedAt = DateTime.Now.AddDays(-1)  }
    );
    await context.SaveChangesAsync();

    // 8. Utility bills (electricity + water for each tenant, last 2 months)
    var billSeed = new List<UtilityBill>();
    for (int i = 0; i < tenants.Count; i++)
    {
        var t  = tenants[i];
        var u  = allUnits[i];
        // Last month
        billSeed.Add(new UtilityBill { ServiceType = "Electricity", PreviousReading = 1000 + i * 50, CurrentReading = 1120 + i * 50, TotalAmount = 180 + i * 5, IssueDate = DateTime.Now.AddMonths(-1), Status = "Paid",   UnitId = u.Id, TenantId = t.Id });
        billSeed.Add(new UtilityBill { ServiceType = "Water",       PreviousReading = 200  + i * 10, CurrentReading = 230  + i * 10, TotalAmount = 60  + i * 2, IssueDate = DateTime.Now.AddMonths(-1), Status = "Paid",   UnitId = u.Id, TenantId = t.Id });
        // Current month
        billSeed.Add(new UtilityBill { ServiceType = "Electricity", PreviousReading = 1120 + i * 50, CurrentReading = 1240 + i * 50, TotalAmount = 190 + i * 5, IssueDate = DateTime.Now,               Status = "Unpaid", UnitId = u.Id, TenantId = t.Id });
        billSeed.Add(new UtilityBill { ServiceType = "Water",       PreviousReading = 230  + i * 10, CurrentReading = 260  + i * 10, TotalAmount = 65  + i * 2, IssueDate = DateTime.Now,               Status = "Unpaid", UnitId = u.Id, TenantId = t.Id });
    }
    context.UtilityBills.AddRange(billSeed);
    await context.SaveChangesAsync();
}

await SeedData(app.Services);

app.Run();
