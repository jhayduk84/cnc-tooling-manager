using Microsoft.AspNetCore.Mvc;
using CncTooling.Application.Services;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController : ControllerBase
{
    private readonly IOperatorService _operatorService;

    public PartsController(IOperatorService operatorService)
    {
        _operatorService = operatorService;
    }

    [HttpGet("{partNumber}")]
    public async Task<IActionResult> GetPart(string partNumber)
    {
        var part = await _operatorService.GetPartByNumberAsync(partNumber);
        if (part == null)
            return NotFound(new { message = $"Part {partNumber} not found" });

        return Ok(part);
    }

    [HttpGet("{partNumber}/operations")]
    public async Task<IActionResult> GetPartOperations(string partNumber, [FromQuery] string? revision = null)
    {
        var operations = await _operatorService.GetPartOperationsAsync(partNumber, revision);
        if (operations == null || !operations.Any())
            return NotFound(new { message = $"No operations found for part {partNumber}" });

        return Ok(operations);
    }
}
