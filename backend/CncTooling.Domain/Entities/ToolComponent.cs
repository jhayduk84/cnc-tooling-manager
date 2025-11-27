namespace CncTooling.Domain.Entities;

public class ToolComponent
{
    public int ToolComponentId { get; set; }
    public string ComponentType { get; set; } = string.Empty; // Cutter, Holder, Collet, Extension, Insert, Misc
    public string ComponentCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? AssetTag { get; set; }
    public decimal? UnitCost { get; set; }
    public bool IsActive { get; set; } = true;
    public string? EspritToolId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ToolAssemblyComponent> ToolAssemblyComponents { get; set; } = new List<ToolAssemblyComponent>();
    public ICollection<ComponentInventoryStatus> InventoryStatuses { get; set; } = new List<ComponentInventoryStatus>();
}
