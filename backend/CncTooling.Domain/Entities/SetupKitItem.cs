namespace CncTooling.Domain.Entities;

public class SetupKitItem
{
    public int SetupKitItemId { get; set; }
    public int SetupKitId { get; set; }
    public int? ToolAssemblyId { get; set; }
    public int? ToolComponentId { get; set; }
    public int QuantityPlanned { get; set; } = 1;
    public int QuantityPulled { get; set; } = 0;
    public string Status { get; set; } = "Pending"; // Pending, Pulled, Verified, Missing
    public string? Notes { get; set; }

    // Navigation properties
    public SetupKit SetupKit { get; set; } = null!;
    public ToolAssembly? ToolAssembly { get; set; }
    public ToolComponent? ToolComponent { get; set; }
}
