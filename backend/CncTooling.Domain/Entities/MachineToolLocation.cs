namespace CncTooling.Domain.Entities;

public class MachineToolLocation
{
    public int Id { get; set; }
    public int MachineToolLocationId { get; set; }
    public int MachineId { get; set; }
    public Machine Machine { get; set; } = null!;
    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;
    public int? ToolAssemblyId { get; set; }
    public ToolAssembly? ToolAssembly { get; set; }
    public int? ToolComponentId { get; set; }
    public ToolComponent? ToolComponent { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public int PocketNumber { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? Notes { get; set; }
}
