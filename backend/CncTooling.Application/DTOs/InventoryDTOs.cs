namespace CncTooling.Application.DTOs;

public class MachineDto
{
    public int MachineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MachineType { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public bool IsActive { get; set; }
}

public class MachineToolLocationDto
{
    public int MachineToolLocationId { get; set; }
    public int MachineId { get; set; }
    public int PocketNumber { get; set; }
    public string? Station { get; set; }
    public int? ToolAssemblyId { get; set; }
    public int? ToolComponentId { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? Notes { get; set; }
    public string? MachineName { get; set; }
    public string? ToolAssemblyName { get; set; }
}

public class InventoryLocationDto
{
    public int InventoryLocationId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class SetupKitDto
{
    public int SetupKitId { get; set; }
    public string KitName { get; set; } = string.Empty;
    public int? PartRevisionId { get; set; }
    public int? OperationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<SetupKitItemDto> Items { get; set; } = new();
}

public class SetupKitItemDto
{
    public int SetupKitItemId { get; set; }
    public int SetupKitId { get; set; }
    public int? ToolAssemblyId { get; set; }
    public int? ToolComponentId { get; set; }
    public int QuantityPlanned { get; set; }
    public int QuantityPulled { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
