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
    public DateTime CreatedAt { get; set; }
}
