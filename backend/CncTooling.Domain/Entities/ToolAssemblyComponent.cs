namespace CncTooling.Domain.Entities;

public class ToolAssemblyComponent
{
    public int ToolAssemblyComponentId { get; set; }
    public int ToolAssemblyId { get; set; }
    public int ToolComponentId { get; set; }
    public int QuantityRequired { get; set; } = 1;
    public bool IsPrimary { get; set; } = false;
    public string? Notes { get; set; }

    // Navigation properties
    public ToolAssembly ToolAssembly { get; set; } = null!;
    public ToolComponent ToolComponent { get; set; } = null!;
}
