namespace CncTooling.Domain.Entities;

public class PartRevision
{
    public int PartRevisionId { get; set; }
    public int PartId { get; set; }
    public string RevisionCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Part Part { get; set; } = null!;
    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
}
