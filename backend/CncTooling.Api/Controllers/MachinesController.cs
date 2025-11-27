using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CncTooling.Application.DTOs;
using CncTooling.Domain.Entities;
using CncTooling.Infrastructure.Data;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MachinesController : ControllerBase
{
    private readonly CncToolingDbContext _context;

    public MachinesController(CncToolingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = _context.Machines.AsQueryable();
        if (activeOnly)
            query = query.Where(m => m.IsActive);

        var machines = await query.ToListAsync();
        return Ok(machines.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null)
            return NotFound();

        return Ok(MapToDto(machine));
    }

    [HttpGet("{id}/tool-locations")]
    public async Task<IActionResult> GetToolLocations(int id)
    {
        var locations = await _context.MachineToolLocations
            .Include(l => l.Machine)
            .Include(l => l.ToolAssembly)
            .Where(l => l.MachineId == id)
            .OrderBy(l => l.PocketNumber)
            .ToListAsync();

        return Ok(locations.Select(MapLocationToDto));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MachineDto dto)
    {
        var machine = new Machine
        {
            Name = dto.Name,
            Description = dto.Description,
            MachineType = dto.MachineType,
            Manufacturer = dto.Manufacturer,
            Model = dto.Model,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Machines.Add(machine);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = machine.MachineId }, MapToDto(machine));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MachineDto dto)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null)
            return NotFound();

        machine.Name = dto.Name;
        machine.Description = dto.Description;
        machine.MachineType = dto.MachineType;
        machine.Manufacturer = dto.Manufacturer;
        machine.Model = dto.Model;
        machine.IsActive = dto.IsActive;
        machine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(machine));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var machine = await _context.Machines.FindAsync(id);
        if (machine == null)
            return NotFound();

        machine.IsActive = false;
        machine.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static MachineDto MapToDto(Machine machine)
    {
        return new MachineDto
        {
            MachineId = machine.MachineId,
            Name = machine.Name,
            Description = machine.Description,
            MachineType = machine.MachineType,
            Manufacturer = machine.Manufacturer,
            Model = machine.Model,
            IsActive = machine.IsActive
        };
    }

    private static MachineToolLocationDto MapLocationToDto(MachineToolLocation location)
    {
        return new MachineToolLocationDto
        {
            MachineToolLocationId = location.MachineToolLocationId,
            MachineId = location.MachineId,
            PocketNumber = location.PocketNumber,
            Station = location.Station,
            ToolAssemblyId = location.ToolAssemblyId,
            ToolComponentId = location.ToolComponentId,
            LastUpdated = location.LastUpdated,
            Notes = location.Notes,
            MachineName = location.Machine?.Name,
            ToolAssemblyName = location.ToolAssembly?.AssemblyName
        };
    }
}
