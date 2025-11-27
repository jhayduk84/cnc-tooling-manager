namespace CncTooling.Application.DTOs;

public class PartDto
{
    public int PartId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? DefaultRevisionId { get; set; }
    public bool IsActive { get; set; }
    public PartRevisionDto? DefaultRevision { get; set; }
    public List<PartRevisionDto> Revisions { get; set; } = new();
}

public class PartRevisionDto
{
    public int PartRevisionId { get; set; }
    public int PartId { get; set; }
    public string RevisionCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public List<OperationDto> Operations { get; set; } = new();
}

public class OperationDto
{
    public int OperationId { get; set; }
    public int PartRevisionId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public string? EspritProgramName { get; set; }
    public string? EspritProgramId { get; set; }
    public string? SetupSheetPath { get; set; }
    public string? SetupSheetUrl { get; set; }
    public bool IsActive { get; set; }
    public int SequenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class SetupSheetDto
{
    public int SetupSheetId { get; set; }
    public int OperationId { get; set; }
    public string? FilePath { get; set; }
    public string? Url { get; set; }
    public string Format { get; set; } = "PDF";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
