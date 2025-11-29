namespace CncTooling.Domain.Entities;

public class ToolAssembly
{
    public int Id { get; set; }
    public int ToolAssemblyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AssemblyName { get; set; } = string.Empty;
    public string? EspritToolId { get; set; }
    public string ToolNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<ToolAssemblyComponent> Components { get; set; } = new List<ToolAssemblyComponent>();
    public ICollection<ToolAssemblyComponent> ToolAssemblyComponents { get; set; } = new List<ToolAssemblyComponent>();
}
