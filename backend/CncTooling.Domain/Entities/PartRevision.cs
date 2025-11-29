namespace CncTooling.Domain.Entities;

public class PartRevision
{
    public int Id { get; set; }
    public int PartRevisionId { get; set; }
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
    public string RevisionNumber { get; set; } = string.Empty;
    public string RevisionCode { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public ICollection<Operation> Operations { get; set; } = new List<Operation>();
}
