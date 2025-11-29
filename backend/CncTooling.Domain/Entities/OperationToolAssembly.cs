namespace CncTooling.Domain.Entities;

public class OperationToolAssembly
{
    public int Id { get; set; }
    public int OperationId { get; set; }
    public Operation Operation { get; set; } = null!;
    public int ToolAssemblyId { get; set; }
    public ToolAssembly ToolAssembly { get; set; } = null!;
    public int QuantityRequired { get; set; }
}
