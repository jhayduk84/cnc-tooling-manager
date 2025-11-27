using Microsoft.EntityFrameworkCore;
using CncTooling.Domain.Entities;

namespace CncTooling.Infrastructure.Data;

public class CncToolingDbContext : DbContext
{
    public CncToolingDbContext(DbContextOptions<CncToolingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Part> Parts { get; set; }
    public DbSet<PartRevision> PartRevisions { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<SetupSheet> SetupSheets { get; set; }
    public DbSet<ToolComponent> ToolComponents { get; set; }
    public DbSet<ToolAssembly> ToolAssemblies { get; set; }
    public DbSet<ToolAssemblyComponent> ToolAssemblyComponents { get; set; }
    public DbSet<OperationToolAssembly> OperationToolAssemblies { get; set; }
    public DbSet<Machine> Machines { get; set; }
    public DbSet<MachineToolLocation> MachineToolLocations { get; set; }
    public DbSet<SetupKit> SetupKits { get; set; }
    public DbSet<SetupKitItem> SetupKitItems { get; set; }
    public DbSet<InventoryLocation> InventoryLocations { get; set; }
    public DbSet<ComponentInventoryStatus> ComponentInventoryStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Part configuration
        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId);
            entity.HasIndex(e => e.PartNumber).IsUnique();
            entity.Property(e => e.PartNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.DefaultRevision)
                .WithMany()
                .HasForeignKey(e => e.DefaultRevisionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // PartRevision configuration
        modelBuilder.Entity<PartRevision>(entity =>
        {
            entity.HasKey(e => e.PartRevisionId);
            entity.HasIndex(e => new { e.PartId, e.RevisionCode }).IsUnique();
            entity.Property(e => e.RevisionCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Part)
                .WithMany(p => p.Revisions)
                .HasForeignKey(e => e.PartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Operation configuration
        modelBuilder.Entity<Operation>(entity =>
        {
            entity.HasKey(e => e.OperationId);
            entity.Property(e => e.OperationName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.EspritProgramName).HasMaxLength(100);
            entity.Property(e => e.EspritProgramId).HasMaxLength(50);
            entity.Property(e => e.SetupSheetPath).HasMaxLength(500);
            entity.Property(e => e.SetupSheetUrl).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.PartRevision)
                .WithMany(p => p.Operations)
                .HasForeignKey(e => e.PartRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SetupSheet configuration
        modelBuilder.Entity<SetupSheet>(entity =>
        {
            entity.HasKey(e => e.SetupSheetId);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.Format).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Operation)
                .WithMany(o => o.SetupSheets)
                .HasForeignKey(e => e.OperationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ToolComponent configuration
        modelBuilder.Entity<ToolComponent>(entity =>
        {
            entity.HasKey(e => e.ToolComponentId);
            entity.HasIndex(e => e.ComponentCode);
            entity.HasIndex(e => e.AssetTag).IsUnique().HasFilter("[AssetTag] IS NOT NULL");
            entity.Property(e => e.ComponentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ComponentCode).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.AssetTag).HasMaxLength(50);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.EspritToolId).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ToolAssembly configuration
        modelBuilder.Entity<ToolAssembly>(entity =>
        {
            entity.HasKey(e => e.ToolAssemblyId);
            entity.HasIndex(e => e.AssemblyName);
            entity.Property(e => e.AssemblyName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EspritToolId).HasMaxLength(50);
            entity.Property(e => e.ToolNumber).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ToolAssemblyComponent configuration (many-to-many)
        modelBuilder.Entity<ToolAssemblyComponent>(entity =>
        {
            entity.HasKey(e => e.ToolAssemblyComponentId);
            entity.HasIndex(e => new { e.ToolAssemblyId, e.ToolComponentId });
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.ToolAssembly)
                .WithMany(a => a.ToolAssemblyComponents)
                .HasForeignKey(e => e.ToolAssemblyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ToolComponent)
                .WithMany(c => c.ToolAssemblyComponents)
                .HasForeignKey(e => e.ToolComponentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OperationToolAssembly configuration (many-to-many)
        modelBuilder.Entity<OperationToolAssembly>(entity =>
        {
            entity.HasKey(e => e.OperationToolAssemblyId);
            entity.HasIndex(e => new { e.OperationId, e.ToolAssemblyId });
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Operation)
                .WithMany(o => o.OperationToolAssemblies)
                .HasForeignKey(e => e.OperationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ToolAssembly)
                .WithMany(a => a.OperationToolAssemblies)
                .HasForeignKey(e => e.ToolAssemblyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Machine configuration
        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(e => e.MachineId);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MachineType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // MachineToolLocation configuration
        modelBuilder.Entity<MachineToolLocation>(entity =>
        {
            entity.HasKey(e => e.MachineToolLocationId);
            entity.HasIndex(e => new { e.MachineId, e.PocketNumber }).IsUnique();
            entity.Property(e => e.Station).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Machine)
                .WithMany(m => m.MachineToolLocations)
                .HasForeignKey(e => e.MachineId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ToolAssembly)
                .WithMany(a => a.MachineToolLocations)
                .HasForeignKey(e => e.ToolAssemblyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ToolComponent)
                .WithMany()
                .HasForeignKey(e => e.ToolComponentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SetupKit configuration
        modelBuilder.Entity<SetupKit>(entity =>
        {
            entity.HasKey(e => e.SetupKitId);
            entity.Property(e => e.KitName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.PartRevision)
                .WithMany()
                .HasForeignKey(e => e.PartRevisionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Operation)
                .WithMany()
                .HasForeignKey(e => e.OperationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SetupKitItem configuration
        modelBuilder.Entity<SetupKitItem>(entity =>
        {
            entity.HasKey(e => e.SetupKitItemId);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.SetupKit)
                .WithMany(k => k.SetupKitItems)
                .HasForeignKey(e => e.SetupKitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ToolAssembly)
                .WithMany()
                .HasForeignKey(e => e.ToolAssemblyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ToolComponent)
                .WithMany()
                .HasForeignKey(e => e.ToolComponentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // InventoryLocation configuration
        modelBuilder.Entity<InventoryLocation>(entity =>
        {
            entity.HasKey(e => e.InventoryLocationId);
            entity.HasIndex(e => e.LocationCode).IsUnique();
            entity.Property(e => e.LocationCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LocationType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ComponentInventoryStatus configuration
        modelBuilder.Entity<ComponentInventoryStatus>(entity =>
        {
            entity.HasKey(e => e.ComponentInventoryStatusId);
            entity.HasIndex(e => new { e.ToolComponentId, e.Status });
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.LastMovementAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.ToolComponent)
                .WithMany(c => c.InventoryStatuses)
                .HasForeignKey(e => e.ToolComponentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InventoryLocation)
                .WithMany(l => l.ComponentInventoryStatuses)
                .HasForeignKey(e => e.InventoryLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Machine)
                .WithMany()
                .HasForeignKey(e => e.MachineId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.MachineToolLocation)
                .WithMany()
                .HasForeignKey(e => e.MachineToolLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SetupKit)
                .WithMany()
                .HasForeignKey(e => e.SetupKitId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
