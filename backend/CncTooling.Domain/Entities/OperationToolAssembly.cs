namespace CncTooling.Domain.Entities;

public class OperationToolAssembly
{
    public int OperationToolAssemblyId { get; set; }
    public int OperationId { get; set; }
    public int ToolAssemblyId { get; set; }
    public int QuantityRequired { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public Operation Operation { get; set; } = null!;
    public ToolAssembly ToolAssembly { get; set; } = null!;
}
