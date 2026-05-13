using Microsoft.EntityFrameworkCore;
using Propertify.Web.Models;

namespace Propertify.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<UtilityBill> UtilityBills { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BookingRequest> BookingRequests { get; set; }
        public DbSet<SystemMessage> SystemMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent cascade delete when unit is deleted (tenant still references it)
            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.Unit)
                .WithMany()
                .HasForeignKey(t => t.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent cascade delete when property is deleted (maintenance requests reference it)
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(m => m.Property)
                .WithMany()
                .HasForeignKey(m => m.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRequest>()
                .Property(m => m.Cost)
                .HasPrecision(18, 3);

            // User -> Tenant: one user account per tenant, no cascade delete
            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Unique email per user account
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Indexes on FK columns for join/filter performance
            modelBuilder.Entity<Unit>().HasIndex(u => u.PropertyId);
            modelBuilder.Entity<Tenant>().HasIndex(t => t.UnitId);
            modelBuilder.Entity<UtilityBill>().HasIndex(b => b.UnitId);
            modelBuilder.Entity<UtilityBill>().HasIndex(b => b.TenantId);
            modelBuilder.Entity<MaintenanceRequest>().HasIndex(m => m.PropertyId);
            modelBuilder.Entity<MaintenanceRequest>().HasIndex(m => m.UnitId);
            modelBuilder.Entity<Contract>().HasIndex(c => c.TenantId);
            modelBuilder.Entity<Contract>().HasIndex(c => c.UnitId);
            modelBuilder.Entity<User>().HasIndex(u => u.TenantId);
        }
    }
}
