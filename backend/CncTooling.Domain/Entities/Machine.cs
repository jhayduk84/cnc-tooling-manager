namespace CncTooling.Domain.Entities;

public class Machine
{
    public int MachineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MachineType { get; set; } = string.Empty; // 3-axis mill, 5-axis, etc.
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<MachineToolLocation> MachineToolLocations { get; set; } = new List<MachineToolLocation>();
}
