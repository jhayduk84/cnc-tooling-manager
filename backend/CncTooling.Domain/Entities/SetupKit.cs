namespace CncTooling.Domain.Entities;

public class SetupKit
{
    public int SetupKitId { get; set; }
    public string KitName { get; set; } = string.Empty;
    public int? PartRevisionId { get; set; }
    public int? OperationId { get; set; }
    public string Status { get; set; } = "Planned"; // Planned, InProgress, Complete, Archived
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public PartRevision? PartRevision { get; set; }
    public Operation? Operation { get; set; }
    public ICollection<SetupKitItem> SetupKitItems { get; set; } = new List<SetupKitItem>();
}
