namespace CncTooling.Domain.Entities;

public class ToolComponent
{
    public int Id { get; set; }
    public int ToolComponentId { get; set; }
    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;
    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;
    public int Quantity { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string AssetTag { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public bool IsActive { get; set; } = true;
    public string? EspritToolId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
