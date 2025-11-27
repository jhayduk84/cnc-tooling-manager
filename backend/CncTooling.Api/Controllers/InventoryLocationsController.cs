using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CncTooling.Application.DTOs;
using CncTooling.Domain.Entities;
using CncTooling.Infrastructure.Data;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryLocationsController : ControllerBase
{
    private readonly CncToolingDbContext _context;

    public InventoryLocationsController(CncToolingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = _context.InventoryLocations.AsQueryable();
        if (activeOnly)
            query = query.Where(l => l.IsActive);

        var locations = await query.ToListAsync();
        return Ok(locations.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var location = await _context.InventoryLocations.FindAsync(id);
        if (location == null)
            return NotFound();

        return Ok(MapToDto(location));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InventoryLocationDto dto)
    {
        var location = new InventoryLocation
        {
            LocationCode = dto.LocationCode,
            Description = dto.Description,
            LocationType = dto.LocationType,
            IsActive = dto.IsActive,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.InventoryLocations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = location.InventoryLocationId }, MapToDto(location));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] InventoryLocationDto dto)
    {
        var location = await _context.InventoryLocations.FindAsync(id);
        if (location == null)
            return NotFound();

        location.LocationCode = dto.LocationCode;
        location.Description = dto.Description;
        location.LocationType = dto.LocationType;
        location.IsActive = dto.IsActive;
        location.Notes = dto.Notes;
        location.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(location));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.InventoryLocations.FindAsync(id);
        if (location == null)
            return NotFound();

        location.IsActive = false;
        location.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static InventoryLocationDto MapToDto(InventoryLocation location)
    {
        return new InventoryLocationDto
        {
            InventoryLocationId = location.InventoryLocationId,
            LocationCode = location.LocationCode,
            Description = location.Description,
            LocationType = location.LocationType,
            IsActive = location.IsActive,
            Notes = location.Notes
        };
    }
}
