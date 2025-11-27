namespace CncTooling.Domain.Entities;

public class Operation
{
    public int OperationId { get; set; }
    public int PartRevisionId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public string? EspritProgramName { get; set; }
    public string? EspritProgramId { get; set; }
    public string? SetupSheetPath { get; set; }
    public string? SetupSheetUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int SequenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public PartRevision PartRevision { get; set; } = null!;
    public ICollection<SetupSheet> SetupSheets { get; set; } = new List<SetupSheet>();
    public ICollection<OperationToolAssembly> OperationToolAssemblies { get; set; } = new List<OperationToolAssembly>();
}
