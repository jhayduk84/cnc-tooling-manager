using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CncTooling.Infrastructure.Data;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetupSheetParserController : ControllerBase
{
    private readonly CncToolingDbContext _context;
    private readonly IConfiguration _configuration;

    public SetupSheetParserController(CncToolingDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    [HttpPost("parse/{setupSheetId}")]
    public async Task<IActionResult> ParseSetupSheet(int setupSheetId)
    {
        try
        {
            var setupSheet = await _context.SetupSheets.FindAsync(setupSheetId);
            if (setupSheet == null)
                return NotFound("Setup sheet not found");

            if (string.IsNullOrEmpty(setupSheet.FilePath))
                return BadRequest("Setup sheet has no file path");

            // Convert Windows path to container path if needed
            var filePath = ConvertToContainerPath(setupSheet.FilePath);

            if (!System.IO.File.Exists(filePath))
                return NotFound($"File not found: {filePath}");

            var tools = ParseToolsFromExcel(filePath);
            return Ok(new
            {
                setupSheetId,
                toolCount = tools.Count,
                tools
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                error = ex.Message,
                stackTrace = ex.StackTrace,
                innerError = ex.InnerException?.Message
            });
        }
    }

    [HttpPost("import/{setupSheetId}")]
    public async Task<IActionResult> ImportToolsFromSetupSheet(int setupSheetId)
    {
        var setupSheet = await _context.SetupSheets.FindAsync(setupSheetId);
        if (setupSheet == null)
            return NotFound("Setup sheet not found");

        if (string.IsNullOrEmpty(setupSheet.FilePath))
            return BadRequest("Setup sheet has no file path");

        var filePath = ConvertToContainerPath(setupSheet.FilePath);

        if (!System.IO.File.Exists(filePath))
            return NotFound($"File not found: {filePath}");

        try
        {
            var tools = ParseToolsFromExcel(filePath);
            var importedCount = await ImportToolsToOperation(setupSheet.OperationId, tools);

            return Ok(new
            {
                setupSheetId,
                operationId = setupSheet.OperationId,
                toolsFound = tools.Count,
                toolsImported = importedCount,
                tools
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    private List<ParsedTool> ParseToolsFromExcel(string filePath)
    {
        var tools = new List<ParsedTool>();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[0]; // First sheet

        // Find the header row with "Tool" or "Tooling Setup"
        int headerRow = FindHeaderRow(worksheet);
        if (headerRow == 0)
            throw new Exception("Could not find tool list header in setup sheet");

        // Find column indices
        var columns = FindColumnIndices(worksheet, headerRow);

        if (columns.ToolNumberCol == 0)
            throw new Exception("Required column 'Tool #' not found in setup sheet");

        // Parse tool rows (start from row after header)
        for (int row = headerRow + 1; row <= worksheet.Dimension.End.Row; row++)
        {
            var toolNumber = worksheet.Cells[row, columns.ToolNumberCol]?.Value?.ToString()?.Trim();
            
            // Skip empty rows or rows without tool numbers
            if (string.IsNullOrWhiteSpace(toolNumber))
                continue;

            // Stop if we hit another section (like "Comments" or empty rows)
            if (toolNumber.ToLower().Contains("comment") || 
                toolNumber.ToLower().Contains("note") ||
                toolNumber.ToLower().Contains("replace"))
                break;

            var tool = new ParsedTool
            {
                ToolNumber = toolNumber,
                Description = columns.DescriptionCol > 0 
                    ? worksheet.Cells[row, columns.DescriptionCol]?.Value?.ToString()?.Trim() ?? ""
                    : "",
                ManufacturerPartNumber = columns.ManufacturerCol > 0
                    ? worksheet.Cells[row, columns.ManufacturerCol]?.Value?.ToString()?.Trim() ?? ""
                    : "",
                ToolLife = columns.ToolLifeCol > 0
                    ? worksheet.Cells[row, columns.ToolLifeCol]?.Value?.ToString()?.Trim() ?? ""
                    : "",
                Holder = columns.HolderCol > 0
                    ? worksheet.Cells[row, columns.HolderCol]?.Value?.ToString()?.Trim() ?? ""
                    : "",
                StickOut = columns.StickOutCol > 0
                    ? worksheet.Cells[row, columns.StickOutCol]?.Value?.ToString()?.Trim() ?? ""
                    : ""
            };

            tools.Add(tool);
        }

        return tools;
    }

    private int FindHeaderRow(ExcelWorksheet worksheet)
    {
        // Look for "Tooling Setup" or similar header, then return the next row with actual column headers
        for (int row = 1; row <= Math.Min(20, worksheet.Dimension.End.Row); row++)
        {
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var cellValue = worksheet.Cells[row, col].Value?.ToString()?.ToLower() ?? "";
                
                // If we find "Tooling Setup", the actual column headers are in the next row
                if (cellValue.Contains("tooling setup"))
                {
                    return row + 1; // Return the row after "Tooling Setup"
                }
            }
        }
        
        // Fallback: Look for a row with "#" or "Tool #" in first few columns
        for (int row = 1; row <= Math.Min(20, worksheet.Dimension.End.Row); row++)
        {
            for (int col = 1; col <= 3; col++)
            {
                var cellValue = worksheet.Cells[row, col].Value?.ToString()?.ToLower() ?? "";
                if (cellValue == "#" || cellValue == "tool #" || cellValue == "t#")
                {
                    return row;
                }
            }
        }
        
        return 0;
    }

    private ColumnIndices FindColumnIndices(ExcelWorksheet worksheet, int headerRow)
    {
        var indices = new ColumnIndices();

        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            var header = worksheet.Cells[headerRow, col].Value?.ToString()?.ToLower()?.Trim() ?? "";

            // Column 1 with just "#" is the tool number
            if (header == "#" || header == "tool #" || header == "t#")
                indices.ToolNumberCol = col;
            else if (header == "tool" || (header.Contains("tool") && header.Contains("descrip")))
                indices.DescriptionCol = col;
            else if (header.Contains("manufacture") || header.Contains("mfg") || header == "manufacture #")
                indices.ManufacturerCol = col;
            else if (header.Contains("tool life") || header == "tool life")
                indices.ToolLifeCol = col;
            else if (header.Contains("holder"))
                indices.HolderCol = col;
            else if (header.Contains("stickout") || header.Contains("stick"))
                indices.StickOutCol = col;
        }

        return indices;
    }

    private async Task<int> ImportToolsToOperation(int operationId, List<ParsedTool> tools)
    {
        int importedCount = 0;

        foreach (var tool in tools)
        {
            // Try to find or create tool component
            var componentCode = tool.ManufacturerPartNumber;
            if (string.IsNullOrWhiteSpace(componentCode))
                componentCode = tool.Description;

            // Check if component exists
            var existingComponent = _context.ToolComponents
                .FirstOrDefault(tc => tc.ComponentCode == componentCode);

            if (existingComponent == null)
            {
                // Create new component
                var newComponent = new Domain.Entities.ToolComponent
                {
                    ComponentCode = componentCode,
                    ComponentType = DetermineComponentType(tool.Description),
                    Description = tool.Description,
                    Manufacturer = ExtractManufacturer(tool.ManufacturerPartNumber),
                    AssetTag = tool.ToolNumber,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ToolComponents.Add(newComponent);
                await _context.SaveChangesAsync();
                existingComponent = newComponent;
            }

            // Create tool assembly for this tool
            var assembly = new Domain.Entities.ToolAssembly
            {
                AssemblyName = $"T{tool.ToolNumber} - {tool.Description}",
                Description = tool.Description,
                ToolNumber = tool.ToolNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.ToolAssemblies.Add(assembly);
            await _context.SaveChangesAsync();

            // Link component to assembly
            var assemblyComponent = new Domain.Entities.ToolAssemblyComponent
            {
                ToolAssemblyId = assembly.ToolAssemblyId,
                ToolComponentId = existingComponent.ToolComponentId,
                QuantityRequired = 1,
                IsPrimary = true
            };
            _context.ToolAssemblyComponents.Add(assemblyComponent);

            // Link assembly to operation
            var operationToolAssembly = new Domain.Entities.OperationToolAssembly
            {
                OperationId = operationId,
                ToolAssemblyId = assembly.ToolAssemblyId,
                QuantityRequired = 1
            };
            _context.OperationToolAssemblies.Add(operationToolAssembly);

            await _context.SaveChangesAsync();
            importedCount++;
        }

        return importedCount;
    }

    private string DetermineComponentType(string description)
    {
        var desc = description.ToLower();
        if (desc.Contains("drill")) return "Drill";
        if (desc.Contains("endmill") || desc.Contains("end mill")) return "Endmill";
        if (desc.Contains("tap")) return "Tap";
        if (desc.Contains("bore") || desc.Contains("boring")) return "Boring Bar";
        if (desc.Contains("reamer")) return "Reamer";
        if (desc.Contains("holder") || desc.Contains("chuck")) return "Holder";
        if (desc.Contains("collet")) return "Collet";
        return "Other";
    }

    private string ExtractManufacturer(string manufacturerPartNumber)
    {
        if (string.IsNullOrWhiteSpace(manufacturerPartNumber))
            return "";

        // Common manufacturer patterns
        if (manufacturerPartNumber.StartsWith("GARR", StringComparison.OrdinalIgnoreCase))
            return "GARR";
        if (manufacturerPartNumber.StartsWith("OSG", StringComparison.OrdinalIgnoreCase))
            return "OSG";
        if (manufacturerPartNumber.Contains("KEO", StringComparison.OrdinalIgnoreCase))
            return "KEO";
        if (manufacturerPartNumber.Contains("SANDVIK", StringComparison.OrdinalIgnoreCase) ||
            manufacturerPartNumber.StartsWith("C", StringComparison.OrdinalIgnoreCase))
            return "Sandvik";

        // Extract first word as manufacturer
        var parts = manufacturerPartNumber.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "";
    }

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
}

public class ParsedTool
{
    public string ToolNumber { get; set; } = "";
    public string Description { get; set; } = "";
    public string ManufacturerPartNumber { get; set; } = "";
    public string ToolLife { get; set; } = "";
    public string Holder { get; set; } = "";
    public string StickOut { get; set; } = "";
}

public class ColumnIndices
{
    public int ToolNumberCol { get; set; }
    public int DescriptionCol { get; set; }
    public int ManufacturerCol { get; set; }
    public int ToolLifeCol { get; set; }
    public int HolderCol { get; set; }
    public int StickOutCol { get; set; }
}
