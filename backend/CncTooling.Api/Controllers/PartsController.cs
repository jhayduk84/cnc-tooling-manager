using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CncTooling.Application.Services;
using CncTooling.Infrastructure.Data;
using CncTooling.Domain.Entities;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController : ControllerBase
{
    private readonly IOperatorService _operatorService;
    private readonly CncToolingDbContext _context;

    public PartsController(IOperatorService operatorService, CncToolingDbContext context)
    {
        _operatorService = operatorService;
        _context = context;
    }

    // Admin: Get all parts
    [HttpGet]
    public async Task<IActionResult> GetAllParts()
    {
        var parts = await _context.Parts
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(parts);
    }

    // Admin: Get part by ID
    [HttpGet("id/{id}")]
    public async Task<IActionResult> GetPartById(int id)
    {
        var part = await _context.Parts.FindAsync(id);
        if (part == null)
            return NotFound();
        return Ok(part);
    }

    // Operator: Get part by part number
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

    // Admin: Create part
    [HttpPost]
    public async Task<IActionResult> CreatePart([FromBody] Part part)
    {
        part.CreatedAt = DateTime.UtcNow;
        _context.Parts.Add(part);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPartById), new { id = part.PartId }, part);
    }

    // Admin: Update part
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePart(int id, [FromBody] Part part)
    {
        if (id != part.PartId)
            return BadRequest();

        _context.Entry(part).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Parts.AnyAsync(p => p.PartId == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    // Admin: Delete part
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePart(int id)
    {
        var part = await _context.Parts.FindAsync(id);
        if (part == null)
            return NotFound();

        _context.Parts.Remove(part);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
