using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CncTooling.Application.DTOs;
using CncTooling.Domain.Entities;
using CncTooling.Infrastructure.Data;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToolComponentsController : ControllerBase
{
    private readonly CncToolingDbContext _context;

    public ToolComponentsController(CncToolingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = _context.ToolComponents.AsQueryable();
        if (activeOnly)
            query = query.Where(c => c.IsActive);

        var components = await query.ToListAsync();
        return Ok(components.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var component = await _context.ToolComponents.FindAsync(id);
        if (component == null)
            return NotFound();

        return Ok(MapToDto(component));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ToolComponentDto dto)
    {
        var component = new ToolComponent
        {
            ComponentType = dto.ComponentType,
            ComponentCode = dto.ComponentCode,
            Description = dto.Description,
            Manufacturer = dto.Manufacturer,
            AssetTag = dto.AssetTag,
            UnitCost = dto.UnitCost ?? 0,
            IsActive = dto.IsActive,
            EspritToolId = dto.EspritToolId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.ToolComponents.Add(component);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = component.ToolComponentId }, MapToDto(component));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ToolComponentDto dto)
    {
        var component = await _context.ToolComponents.FindAsync(id);
        if (component == null)
            return NotFound();

        component.ComponentType = dto.ComponentType;
        component.ComponentCode = dto.ComponentCode;
        component.Description = dto.Description;
        component.Manufacturer = dto.Manufacturer;
        component.AssetTag = dto.AssetTag;
        component.UnitCost = dto.UnitCost ?? 0;
        component.IsActive = dto.IsActive;
        component.EspritToolId = dto.EspritToolId;
        component.Notes = dto.Notes;
        component.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(component));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var component = await _context.ToolComponents.FindAsync(id);
        if (component == null)
            return NotFound();

        component.IsActive = false;
        component.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ToolComponentDto MapToDto(ToolComponent component)
    {
        return new ToolComponentDto
        {
            ToolComponentId = component.ToolComponentId,
            ComponentType = component.ComponentType,
            ComponentCode = component.ComponentCode,
            Description = component.Description,
            Manufacturer = component.Manufacturer,
            AssetTag = component.AssetTag,
            UnitCost = component.UnitCost,
            IsActive = component.IsActive,
            EspritToolId = component.EspritToolId,
            Notes = component.Notes
        };
    }
}
