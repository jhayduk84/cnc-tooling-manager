namespace CncTooling.Domain.Entities;

public class SetupSheet
{
    public int SetupSheetId { get; set; }
    public int OperationId { get; set; }
    public string? FilePath { get; set; }
    public string? Url { get; set; }
    public string Format { get; set; } = "PDF"; // PDF, HTML, etc.
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Operation Operation { get; set; } = null!;
}
