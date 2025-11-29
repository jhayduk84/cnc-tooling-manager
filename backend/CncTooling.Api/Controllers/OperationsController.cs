using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CncTooling.Application.Services;
using CncTooling.Infrastructure.Data;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperationsController : ControllerBase
{
    private readonly IOperatorService _operatorService;
    private readonly CncToolingDbContext _context;

    public OperationsController(IOperatorService operatorService, CncToolingDbContext context)
    {
        _operatorService = operatorService;
        _context = context;
    }

    // Admin: Get all operations
    [HttpGet]
    public async Task<IActionResult> GetAllOperations()
    {
        var operations = await _context.Operations
            .Include(o => o.PartRevision)
                .ThenInclude(pr => pr.Part)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return Ok(operations);
    }

    [HttpGet("{operationId}")]
    public async Task<IActionResult> GetOperation(int operationId)
    {
        var operation = await _operatorService.GetOperationAsync(operationId);
        if (operation == null)
            return NotFound(new { message = $"Operation {operationId} not found" });

        return Ok(operation);
    }

    [HttpGet("{operationId}/setup-sheet")]
    public async Task<IActionResult> GetSetupSheet(int operationId)
    {
        var setupSheet = await _operatorService.GetSetupSheetAsync(operationId);
        if (setupSheet == null)
            return NotFound(new { message = $"No setup sheet found for operation {operationId}" });

        return Ok(setupSheet);
    }

    [HttpGet("{operationId}/tooling")]
    public async Task<IActionResult> GetOperationTooling(int operationId)
    {
        var tooling = await _operatorService.GetOperationToolingAsync(operationId, includeLocations: false);
        return Ok(tooling);
    }

    [HttpGet("{operationId}/tooling/with-locations")]
    public async Task<IActionResult> GetOperationToolingWithLocations(int operationId)
    {
        var tooling = await _operatorService.GetOperationToolingAsync(operationId, includeLocations: true);
        return Ok(tooling);
    }
}
