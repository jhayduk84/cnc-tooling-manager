namespace CncTooling.Domain.Entities;

public class ComponentInventoryStatus
{
    public int ComponentInventoryStatusId { get; set; }
    public int ToolComponentId { get; set; }
    public int? InventoryLocationId { get; set; }
    public int? MachineId { get; set; }
    public int? MachineToolLocationId { get; set; }
    public int? SetupKitId { get; set; }
    public string Status { get; set; } = "Available"; // Available, InMachine, InSetupKit, OutForRegrind, Scrap, Lost
    public int QuantityOnHand { get; set; } = 1;
    public DateTime LastMovementAt { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public ToolComponent ToolComponent { get; set; } = null!;
    public InventoryLocation? InventoryLocation { get; set; }
    public Machine? Machine { get; set; }
    public MachineToolLocation? MachineToolLocation { get; set; }
    public SetupKit? SetupKit { get; set; }
}
