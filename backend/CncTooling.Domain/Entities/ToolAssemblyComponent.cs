namespace CncTooling.Domain.Entities;

public class ToolAssemblyComponent
{
    public int Id { get; set; }
    public int ToolAssemblyComponentId { get; set; }
    public int ToolAssemblyId { get; set; }
    public ToolAssembly ToolAssembly { get; set; } = null!;
    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;
    public int ToolComponentId { get; set; }
    public ToolComponent ToolComponent { get; set; } = null!;
    public int Quantity { get; set; }
    public int QuantityRequired { get; set; }
    public int Position { get; set; }
    public bool IsPrimary { get; set; }
    public string? Notes { get; set; }
}
