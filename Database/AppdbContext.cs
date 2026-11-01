using IntelligentInventoryDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelligentInventoryDashboard.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.Vehicles> Vehicles => Set<Entities.Vehicles>();

        public DbSet<Entities.VehicleUpdateRequest> VehicleUpdateRequests => Set<Entities.VehicleUpdateRequest>();

        public DbSet<Entities.AgingAction> AgingActions => Set<Entities.AgingAction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Vehicles ----
            modelBuilder.Entity<Entities.Vehicles>(entity =>
            {
                entity.ToTable("Vehicles");

                entity.HasKey(v => v.Id);

                entity.Property(v => v.ChassisNumber)
                      .IsRequired()
                      .HasMaxLength(32);

                // One chassis number identifies at most one vehicle
                entity.HasIndex(v => v.ChassisNumber).IsUnique();

                entity.Property(v => v.Make)
                      .IsRequired()
                      .HasMaxLength(64);

                entity.Property(v => v.Model)
                      .IsRequired()
                      .HasMaxLength(64);

                entity.Property(v => v.Year)
                      .IsRequired();

                entity.Property(v => v.Price)
                      .HasColumnType("decimal(18,2)");

                // EF Core 8+ maps DateOnly natively (DATE on SQL Server / TEXT on SQLite),
                // so no custom HasConversion is required. If you ever target EF Core 6/7,
                // replace this comment with an explicit conversion, e.g.:
                // entity.Property(v => v.ArrivalDate)
                //       .HasConversion(d => d.ToDateTime(TimeOnly.MinValue), dt => DateOnly.FromDateTime(dt));

                entity.Property(v => v.Status)
                      .IsRequired()
                      .HasConversion<string>()
                      .HasMaxLength(20);

                // Relationships: one vehicle has many update requests and aging actions
                entity.HasMany(v => v.UpdateRequests)
                      .WithOne(r => r.Vehicle)
                      .HasForeignKey(r => r.VehicleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.AgingActions)
                      .WithOne(a => a.Vehicle)
                      .HasForeignKey(a => a.VehicleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- VehicleUpdateRequest ----
            modelBuilder.Entity<Entities.VehicleUpdateRequest>(entity =>
            {
                entity.ToTable("VehicleUpdateRequests");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.VehicleId)
                      .IsRequired();

                entity.Property(r => r.ProposedPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(r => r.Reason)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(r => r.RequestedBy)
                      .IsRequired()
                      .HasMaxLength(64);

                entity.Property(r => r.Status)
                      .IsRequired()
                      .HasDefaultValue(UpdateRequestStatus.Pending)
                      .HasConversion<string>()
                      .HasMaxLength(16);

                entity.Property(r => r.RequestedAt)
                      .IsRequired();

                entity.Property(r => r.ApprovedAt);

                entity.HasIndex(r => r.Status);
                entity.HasIndex(r => r.VehicleId);
            });

            // ---- AgingAction ----
            modelBuilder.Entity<Entities.AgingAction>(entity =>
            {
                entity.ToTable("AgingActions");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.VehicleId)
                      .IsRequired();

                entity.Property(a => a.ActionType)
                      .IsRequired()
                      .HasMaxLength(32);

                entity.Property(a => a.Notes)
                      .IsRequired()
                      .HasMaxLength(512);

                entity.Property(a => a.CreatedAt)
                      .IsRequired();

                entity.HasIndex(a => a.VehicleId);
            });
        }
    }
}
