namespace CncTooling.Domain.Entities;

public class Tool
{
    public int Id { get; set; }
    public string ToolNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Diameter { get; set; }
    public decimal Length { get; set; }
    public int ComponentId { get; set; }
    public Component Component { get; set; } = null!;
}