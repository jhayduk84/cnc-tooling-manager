namespace CncTooling.Domain.Entities;

public class InventoryLocation
{
    public int InventoryLocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty; // CribBin, CabinetDrawer, Offsite, Vendor, ScrapBin, etc.
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ComponentInventoryStatus> ComponentInventoryStatuses { get; set; } = new List<ComponentInventoryStatus>();
}
