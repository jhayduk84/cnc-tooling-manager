using System.ComponentModel.DataAnnotations.Schema;

namespace CncTooling.Domain.Entities;

public class SetupSheet
{
    public int Id { get; set; }
    public int SetupSheetId { get; set; }
    public int OperationId { get; set; }
    public Operation Operation { get; set; } = null!;
    public string Instructions { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsArchived { get; set; } = false;
    public string? ArchiveReason { get; set; }
    public int? ReplacedBySetupSheetId { get; set; }
    
    [ForeignKey(nameof(ReplacedBySetupSheetId))]
    public SetupSheet? ReplacedBySetupSheet { get; set; }
    
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
}
