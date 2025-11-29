using CncTooling.Infrastructure.Data;
using CncTooling.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetupSheetsController : ControllerBase
{
    private readonly CncToolingDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public SetupSheetsController(CncToolingDbContext context, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _context = context;
        _environment = environment;
        _configuration = configuration;
    }

    // Helper method to convert Windows paths to Docker container paths
    private string ConvertToContainerPath(string filePath)
    {
        if (filePath.StartsWith("C:\\Users\\", StringComparison.OrdinalIgnoreCase) || 
            filePath.StartsWith("C:/Users/", StringComparison.OrdinalIgnoreCase))
        {
            return filePath.Replace("C:\\Users\\", "/host-users/", StringComparison.OrdinalIgnoreCase)
                          .Replace("C:/Users/", "/host-users/", StringComparison.OrdinalIgnoreCase)
                          .Replace("\\", "/");
        }
        return filePath;
    }

    // Get all setup sheets grouped by part
    [HttpGet("by-part")]
    public async Task<ActionResult> GetSetupSheetsByPart()
    {
        var parts = await _context.Parts
            .Include(p => p.Revisions)
                .ThenInclude(r => r.Operations)
                    .ThenInclude(o => o.SetupSheets.Where(s => !s.IsArchived))
            .Where(p => p.IsActive)
            .ToListAsync();

        return Ok(parts);
    }

    // Get setup sheets for a specific part
    [HttpGet("part/{partId}")]
    public async Task<ActionResult> GetSetupSheetsForPart(int partId)
    {
        var part = await _context.Parts
            .Include(p => p.Revisions)
                .ThenInclude(r => r.Operations)
                    .ThenInclude(o => o.SetupSheets)
            .FirstOrDefaultAsync(p => p.PartId == partId);

        if (part == null)
            return NotFound("Part not found");

        return Ok(part);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SetupSheet>>> GetSetupSheets([FromQuery] bool includeArchived = false)
    {
        var query = _context.SetupSheets
            .Include(s => s.Operation)
                .ThenInclude(o => o.PartRevision)
                    .ThenInclude(pr => pr.Part)
            .AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(s => !s.IsArchived);
        }

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SetupSheet>> GetSetupSheet(int id)
    {
        var setupSheet = await _context.SetupSheets
            .Include(s => s.Operation)
                .ThenInclude(o => o.PartRevision)
                    .ThenInclude(pr => pr.Part)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (setupSheet == null)
        {
            return NotFound();
        }

        return setupSheet;
    }

    // Link existing setup sheet file with part/operation management
    [HttpPost("link")]
    public async Task<ActionResult<SetupSheet>> LinkSetupSheet(
        [FromBody] LinkSetupSheetRequest request)
    {
        if (string.IsNullOrEmpty(request.FilePath))
        {
            return BadRequest("File path is required");
        }

        // Convert Windows path to Docker container path if needed
        var filePath = ConvertToContainerPath(request.FilePath);

        // Validate file exists
        if (!System.IO.File.Exists(filePath))
        {
            return BadRequest($"File not found at: {filePath} (original: {request.FilePath}). Make sure the path is accessible from the server.");
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest("Only Excel files (.xlsx, .xls) are allowed");
        }

        var fileName = Path.GetFileName(filePath);

        // Find or create part
        var part = await _context.Parts
            .Include(p => p.Revisions)
            .FirstOrDefaultAsync(p => p.PartNumber == request.PartNumber);

        if (part == null)
        {
            part = new Part
            {
                PartNumber = request.PartNumber,
                Description = request.PartDescription ?? request.PartNumber,
                Customer = request.Customer,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            // Create default revision
            var revision = new PartRevision
            {
                PartId = part.PartId,
                RevisionNumber = "A",
                RevisionCode = "01",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.PartRevisions.Add(revision);
            await _context.SaveChangesAsync();
        }

        // Get active revision
        var activeRevision = await _context.PartRevisions
            .Where(r => r.PartId == part.PartId && r.IsActive)
            .OrderByDescending(r => r.CreatedDate)
            .FirstOrDefaultAsync();

        if (activeRevision == null)
        {
            return BadRequest("No active revision found for part");
        }

        Operation operation;
        
        if (request.CreateNewOperation)
        {
            // Create new operation
            var maxSequence = await _context.Operations
                .Where(o => o.PartRevisionId == activeRevision.Id)
                .MaxAsync(o => (int?)o.SequenceNumber) ?? 0;

            operation = new Operation
            {
                PartRevisionId = activeRevision.Id,
                OperationName = request.OperationName,
                SequenceNumber = maxSequence + 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Operations.Add(operation);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Find existing operation or create if not found
            operation = await _context.Operations
                .Include(o => o.SetupSheets)
                .FirstOrDefaultAsync(o => 
                    o.PartRevisionId == activeRevision.Id && 
                    o.OperationName == request.OperationName &&
                    o.IsActive);

            if (operation == null)
            {
                var maxSequence = await _context.Operations
                    .Where(o => o.PartRevisionId == activeRevision.Id)
                    .MaxAsync(o => (int?)o.SequenceNumber) ?? 0;

                operation = new Operation
                {
                    PartRevisionId = activeRevision.Id,
                    OperationName = request.OperationName,
                    SequenceNumber = maxSequence + 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Operations.Add(operation);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Archive existing active setup sheets for this operation
                var existingSheets = await _context.SetupSheets
                    .Where(s => s.OperationId == operation.OperationId && !s.IsArchived)
                    .ToListAsync();

                foreach (var sheet in existingSheets)
                {
                    sheet.IsArchived = true;
                    sheet.ArchivedAt = DateTime.UtcNow;
                    sheet.ArchiveReason = request.ArchiveReason ?? "Replaced by new setup sheet";
                }
                
                operation.UpdatedAt = DateTime.UtcNow;
                operation.Notes = request.ArchiveReason;
            }
        }

        // Get version number
        var maxVersion = await _context.SetupSheets
            .Where(s => s.OperationId == operation.OperationId)
            .MaxAsync(s => (int?)s.Version) ?? 0;

        // Create setup sheet with link to existing file
        var setupSheet = new SetupSheet
        {
            OperationId = operation.OperationId,
            Instructions = request.Instructions ?? "",
            FilePath = request.FilePath, // Store the full path to existing file
            Format = extension.TrimStart('.').ToUpperInvariant(),
            Description = fileName,
            IsActive = true,
            IsArchived = false,
            Version = maxVersion + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.SetupSheets.Add(setupSheet);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSetupSheet), new { id = setupSheet.Id }, setupSheet);
    }

    // Upload setup sheet with part/operation management (legacy - keeps file upload option)
    [HttpPost("upload")]
    public async Task<ActionResult<SetupSheet>> UploadSetupSheet(
        [FromForm] IFormFile file,
        [FromForm] string partNumber,
        [FromForm] string? partDescription,
        [FromForm] string operationName,
        [FromForm] string? instructions,
        [FromForm] bool createNewOperation = false,
        [FromForm] string? archiveReason = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest("Only Excel files (.xlsx, .xls) are allowed");
        }

        // Find or create part
        var part = await _context.Parts
            .Include(p => p.Revisions)
            .FirstOrDefaultAsync(p => p.PartNumber == partNumber);

        if (part == null)
        {
            part = new Part
            {
                PartNumber = partNumber,
                Description = partDescription ?? partNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            // Create default revision
            var revision = new PartRevision
            {
                PartId = part.PartId,
                RevisionNumber = "A",
                RevisionCode = "01",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.PartRevisions.Add(revision);
            await _context.SaveChangesAsync();
        }

        // Get active revision
        var activeRevision = await _context.PartRevisions
            .Where(r => r.PartId == part.PartId && r.IsActive)
            .OrderByDescending(r => r.CreatedDate)
            .FirstOrDefaultAsync();

        if (activeRevision == null)
        {
            return BadRequest("No active revision found for part");
        }

        Operation operation;
        
        if (createNewOperation)
        {
            // Create new operation
            var maxSequence = await _context.Operations
                .Where(o => o.PartRevisionId == activeRevision.Id)
                .MaxAsync(o => (int?)o.SequenceNumber) ?? 0;

            operation = new Operation
            {
                PartRevisionId = activeRevision.Id,
                OperationName = operationName,
                SequenceNumber = maxSequence + 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Operations.Add(operation);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Find existing operation or create if not found
            operation = await _context.Operations
                .Include(o => o.SetupSheets)
                .FirstOrDefaultAsync(o => 
                    o.PartRevisionId == activeRevision.Id && 
                    o.OperationName == operationName &&
                    o.IsActive);

            if (operation == null)
            {
                var maxSequence = await _context.Operations
                    .Where(o => o.PartRevisionId == activeRevision.Id)
                    .MaxAsync(o => (int?)o.SequenceNumber) ?? 0;

                operation = new Operation
                {
                    PartRevisionId = activeRevision.Id,
                    OperationName = operationName,
                    SequenceNumber = maxSequence + 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Operations.Add(operation);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Archive existing active setup sheets for this operation
                var existingSheets = await _context.SetupSheets
                    .Where(s => s.OperationId == operation.OperationId && !s.IsArchived)
                    .ToListAsync();

                foreach (var sheet in existingSheets)
                {
                    sheet.IsArchived = true;
                    sheet.ArchivedAt = DateTime.UtcNow;
                    sheet.ArchiveReason = archiveReason ?? "Replaced by new setup sheet";
                }
                
                operation.UpdatedAt = DateTime.UtcNow;
                operation.Notes = archiveReason;
            }
        }

        // Save file
        var useNetworkPath = _configuration.GetValue<bool>("FileStorage:UseNetworkPath");
        var configuredPath = _configuration.GetValue<string>("FileStorage:SetupSheetsPath");
        
        string uploadPath;
        string relativePath;
        
        if (useNetworkPath && !string.IsNullOrEmpty(configuredPath))
        {
            // Use network path
            uploadPath = configuredPath;
            Directory.CreateDirectory(uploadPath);
            relativePath = configuredPath; // Store full network path for retrieval
        }
        else
        {
            // Use local wwwroot path
            uploadPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "setup-sheets");
            Directory.CreateDirectory(uploadPath);
            relativePath = "/setup-sheets"; // Store relative path for web access
        }

        var fileName = $"{partNumber}_{operationName}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Get version number
        var maxVersion = await _context.SetupSheets
            .Where(s => s.OperationId == operation.OperationId)
            .MaxAsync(s => (int?)s.Version) ?? 0;

        // Create setup sheet
        var setupSheet = new SetupSheet
        {
            OperationId = operation.OperationId,
            Instructions = instructions ?? "",
            FilePath = useNetworkPath ? filePath : $"{relativePath}/{fileName}",
            Format = extension.TrimStart('.').ToUpperInvariant(),
            Description = file.FileName,
            IsActive = true,
            IsArchived = false,
            Version = maxVersion + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.SetupSheets.Add(setupSheet);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSetupSheet), new { id = setupSheet.Id }, setupSheet);
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadSetupSheet(int id)
    {
        var setupSheet = await _context.SetupSheets.FindAsync(id);
        if (setupSheet == null)
        {
            return NotFound();
        }

        var useNetworkPath = _configuration.GetValue<bool>("FileStorage:UseNetworkPath");
        string filePath;
        
        if (useNetworkPath || setupSheet.FilePath.StartsWith(@"\\") || Path.IsPathRooted(setupSheet.FilePath))
        {
            // Network path or absolute path stored in database - convert if needed
            filePath = ConvertToContainerPath(setupSheet.FilePath);
        }
        else
        {
            // Relative web path - convert to physical path
            filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", setupSheet.FilePath.TrimStart('/'));
        }
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File not found: {filePath}");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var contentType = setupSheet.Format == "XLSX" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/vnd.ms-excel";
        
        return File(fileBytes, contentType, setupSheet.Description);
    }

    // View/stream setup sheet file (for frontend viewer)
    [HttpGet("view/{id}")]
    public async Task<IActionResult> ViewSetupSheet(int id)
    {
        var setupSheet = await _context.SetupSheets.FindAsync(id);
        if (setupSheet == null)
        {
            return NotFound();
        }

        var useNetworkPath = _configuration.GetValue<bool>("FileStorage:UseNetworkPath");
        string filePath;
        
        if (useNetworkPath || setupSheet.FilePath.StartsWith(@"\\") || Path.IsPathRooted(setupSheet.FilePath))
        {
            // Network path or absolute path stored in database - convert if needed
            filePath = ConvertToContainerPath(setupSheet.FilePath);
        }
        else
        {
            // Relative web path - convert to physical path
            filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", setupSheet.FilePath.TrimStart('/'));
        }
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"File not found: {filePath}");
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var contentType = setupSheet.Format == "XLSX" ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/vnd.ms-excel";
        
        return File(fileBytes, contentType);
    }

    // Archive a setup sheet
    [HttpPost("{id}/archive")]
    public async Task<IActionResult> ArchiveSetupSheet(int id, [FromBody] string reason)
    {
        var setupSheet = await _context.SetupSheets.FindAsync(id);
        if (setupSheet == null)
        {
            return NotFound();
        }

        setupSheet.IsArchived = true;
        setupSheet.ArchivedAt = DateTime.UtcNow;
        setupSheet.ArchiveReason = reason;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSetupSheet(int id, [FromBody] SetupSheet setupSheet)
    {
        if (id != setupSheet.Id)
        {
            return BadRequest();
        }

        _context.Entry(setupSheet).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.SetupSheets.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSetupSheet(int id)
    {
        var setupSheet = await _context.SetupSheets.FindAsync(id);
        if (setupSheet == null)
        {
            return NotFound();
        }

        // Delete file from disk
        var filePath = Path.Combine(_environment.WebRootPath ?? "wwwroot", setupSheet.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _context.SetupSheets.Remove(setupSheet);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// Request model for linking existing files
public class LinkSetupSheetRequest
{
    public required string FilePath { get; set; }
    public required string PartNumber { get; set; }
    public string? PartDescription { get; set; }
    public string? Customer { get; set; }
    public required string OperationName { get; set; }
    public string? Instructions { get; set; }
    public bool CreateNewOperation { get; set; }
    public string? ArchiveReason { get; set; }
}
