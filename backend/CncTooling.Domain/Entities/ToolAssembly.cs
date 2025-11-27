namespace CncTooling.Domain.Entities;

public class ToolAssembly
{
    public int ToolAssemblyId { get; set; }
    public string AssemblyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EspritToolId { get; set; }
    public string? ToolNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ToolAssemblyComponent> ToolAssemblyComponents { get; set; } = new List<ToolAssemblyComponent>();
    public ICollection<OperationToolAssembly> OperationToolAssemblies { get; set; } = new List<OperationToolAssembly>();
    public ICollection<MachineToolLocation> MachineToolLocations { get; set; } = new List<MachineToolLocation>();
}
