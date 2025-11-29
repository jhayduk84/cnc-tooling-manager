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
    public DbSet<Operation> Operations { get; set; }
    public DbSet<Tool> Tools { get; set; }
    public DbSet<Component> Components { get; set; }
    public DbSet<InventoryLocation> InventoryLocations { get; set; }
    public DbSet<ComponentInventoryStatus> ComponentInventoryStatuses { get; set; }
    public DbSet<Machine> Machines { get; set; }
    public DbSet<Operator> Operators { get; set; }
    public DbSet<ToolUsageLog> ToolUsageLogs { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    public DbSet<ToolComponent> ToolComponents { get; set; }
    public DbSet<ToolAssembly> ToolAssemblies { get; set; }
    public DbSet<ToolAssemblyComponent> ToolAssemblyComponents { get; set; }
    public DbSet<PartRevision> PartRevisions { get; set; }
    public DbSet<SetupSheet> SetupSheets { get; set; }
    public DbSet<OperationToolAssembly> OperationToolAssemblies { get; set; }
    public DbSet<MachineToolLocation> MachineToolLocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ignore duplicate navigation properties
        modelBuilder.Entity<Part>()
            .Ignore(p => p.DefaultRevision);
            
        modelBuilder.Entity<ToolAssembly>()
            .Ignore(ta => ta.Components);
        
        // Fix cascade delete conflicts
        modelBuilder.Entity<ToolComponent>()
            .HasOne(tc => tc.Tool)
            .WithMany()
            .HasForeignKey(tc => tc.ToolId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<ToolComponent>()
            .HasOne(tc => tc.Component)
            .WithMany()
            .HasForeignKey(tc => tc.ComponentId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Apply entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CncToolingDbContext).Assembly);
    }
}
