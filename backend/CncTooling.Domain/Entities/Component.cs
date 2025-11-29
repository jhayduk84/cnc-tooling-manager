namespace CncTooling.Domain.Entities;

public class Component
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public ICollection<Tool> Tools { get; set; } = new List<Tool>();
}