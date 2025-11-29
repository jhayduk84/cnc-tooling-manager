namespace CncTooling.Domain.Entities;

public class ToolUsageLog
{
    public int Id { get; set; }
    public int ToolId { get; set; }
    public Tool Tool { get; set; } = null!;
    public DateTime UsedAt { get; set; }
    public int OperatorId { get; set; }
    public Operator Operator { get; set; } = null!;
}