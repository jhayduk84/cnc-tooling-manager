namespace CncTooling.Application.DTOs;

public class ToolComponentDto
{
    public int ToolComponentId { get; set; }
    public string ComponentType { get; set; } = string.Empty;
    public string ComponentCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? AssetTag { get; set; }
    public decimal? UnitCost { get; set; }
    public bool IsActive { get; set; }
    public string? EspritToolId { get; set; }
    public string? Notes { get; set; }
}

public class ToolAssemblyDto
{
    public int ToolAssemblyId { get; set; }
    public string AssemblyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EspritToolId { get; set; }
    public string? ToolNumber { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public List<ToolAssemblyComponentDto> Components { get; set; } = new();
}

public class ToolAssemblyComponentDto
{
    public int ToolAssemblyComponentId { get; set; }
    public int ToolAssemblyId { get; set; }
    public int ToolComponentId { get; set; }
    public int QuantityRequired { get; set; }
    public bool IsPrimary { get; set; }
    public string? Notes { get; set; }
    public ToolComponentDto? Component { get; set; }
}

public class ToolAssemblyAvailabilityDto
{
    public ToolAssemblyDto Assembly { get; set; } = new();
    public string AvailabilityStatus { get; set; } = string.Empty; // FullyAvailable, PartiallyAvailable, NotAvailable
    public int QuantityRequired { get; set; } = 1;
    public List<ComponentAvailabilityDto> ComponentAvailability { get; set; } = new();
}

public class ComponentAvailabilityDto
{
    public ToolComponentDto Component { get; set; } = new();
    public int QuantityRequired { get; set; }
    public int QuantityAvailable { get; set; }
    public bool IsPrimary { get; set; }
    public List<ComponentLocationDto> Locations { get; set; } = new();
}

public class ComponentLocationDto
{
    public string LocationType { get; set; } = string.Empty; // Crib, Machine, SetupKit, Other
    public string LocationDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? MachineInfo { get; set; }
    public int? PocketNumber { get; set; }
}
