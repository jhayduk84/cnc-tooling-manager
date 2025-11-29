namespace CncTooling.Domain.Entities;

public class Part
{
    public int PartId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Customer { get; set; }
    public int? DefaultRevisionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public PartRevision? DefaultRevision { get; set; }
    public ICollection<PartRevision> Revisions { get; set; } = new List<PartRevision>();
}
