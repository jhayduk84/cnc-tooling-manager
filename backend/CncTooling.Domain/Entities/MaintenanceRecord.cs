namespace CncTooling.Domain.Entities;

public class MaintenanceRecord
{
    public int Id { get; set; }
    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;
    public DateTime MaintenanceDate { get; set; }
    public string Description { get; set; } = string.Empty;
}