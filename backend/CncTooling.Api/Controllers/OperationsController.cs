using Microsoft.AspNetCore.Mvc;
using CncTooling.Application.Services;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperationsController : ControllerBase
{
    private readonly IOperatorService _operatorService;

    public OperationsController(IOperatorService operatorService)
    {
        _operatorService = operatorService;
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
