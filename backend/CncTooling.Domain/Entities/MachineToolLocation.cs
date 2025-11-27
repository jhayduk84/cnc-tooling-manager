namespace CncTooling.Domain.Entities;

public class MachineToolLocation
{
    public int MachineToolLocationId { get; set; }
    public int MachineId { get; set; }
    public int PocketNumber { get; set; }
    public string? Station { get; set; }
    public int? ToolAssemblyId { get; set; }
    public int? ToolComponentId { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Machine Machine { get; set; } = null!;
    public ToolAssembly? ToolAssembly { get; set; }
    public ToolComponent? ToolComponent { get; set; }
}
