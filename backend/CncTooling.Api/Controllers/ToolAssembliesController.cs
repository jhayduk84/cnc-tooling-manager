using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CncTooling.Application.DTOs;
using CncTooling.Domain.Entities;
using CncTooling.Infrastructure.Data;

namespace CncTooling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToolAssembliesController : ControllerBase
{
    private readonly CncToolingDbContext _context;

    public ToolAssembliesController(CncToolingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = _context.ToolAssemblies
            .Include(a => a.ToolAssemblyComponents)
                .ThenInclude(c => c.ToolComponent)
            .AsQueryable();

        if (activeOnly)
            query = query.Where(a => a.IsActive);

        var assemblies = await query.ToListAsync();
        return Ok(assemblies.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var assembly = await _context.ToolAssemblies
            .Include(a => a.ToolAssemblyComponents)
                .ThenInclude(c => c.ToolComponent)
            .FirstOrDefaultAsync(a => a.ToolAssemblyId == id);

        if (assembly == null)
            return NotFound();

        return Ok(MapToDto(assembly));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ToolAssemblyDto dto)
    {
        var assembly = new ToolAssembly
        {
            AssemblyName = dto.AssemblyName,
            Description = dto.Description,
            EspritToolId = dto.EspritToolId,
            ToolNumber = dto.ToolNumber,
            IsActive = dto.IsActive,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.ToolAssemblies.Add(assembly);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = assembly.ToolAssemblyId }, MapToDto(assembly));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ToolAssemblyDto dto)
    {
        var assembly = await _context.ToolAssemblies.FindAsync(id);
        if (assembly == null)
            return NotFound();

        assembly.AssemblyName = dto.AssemblyName;
        assembly.Description = dto.Description;
        assembly.EspritToolId = dto.EspritToolId;
        assembly.ToolNumber = dto.ToolNumber;
        assembly.IsActive = dto.IsActive;
        assembly.Notes = dto.Notes;
        assembly.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToDto(assembly));
    }

    [HttpPost("{id}/components")]
    public async Task<IActionResult> AddComponent(int id, [FromBody] ToolAssemblyComponentDto dto)
    {
        var assembly = await _context.ToolAssemblies.FindAsync(id);
        if (assembly == null)
            return NotFound("Assembly not found");

        var component = await _context.ToolComponents.FindAsync(dto.ToolComponentId);
        if (component == null)
            return NotFound("Component not found");

        var assemblyComponent = new ToolAssemblyComponent
        {
            ToolAssemblyId = id,
            ToolComponentId = dto.ToolComponentId,
            QuantityRequired = dto.QuantityRequired,
            IsPrimary = dto.IsPrimary,
            Notes = dto.Notes
        };

        _context.ToolAssemblyComponents.Add(assemblyComponent);
        await _context.SaveChangesAsync();

        return Ok(assemblyComponent);
    }

    [HttpDelete("{id}/components/{componentId}")]
    public async Task<IActionResult> RemoveComponent(int id, int componentId)
    {
        var assemblyComponent = await _context.ToolAssemblyComponents
            .FirstOrDefaultAsync(ac => ac.ToolAssemblyId == id && ac.ToolAssemblyComponentId == componentId);

        if (assemblyComponent == null)
            return NotFound();

        _context.ToolAssemblyComponents.Remove(assemblyComponent);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var assembly = await _context.ToolAssemblies.FindAsync(id);
        if (assembly == null)
            return NotFound();

        assembly.IsActive = false;
        assembly.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ToolAssemblyDto MapToDto(ToolAssembly assembly)
    {
        return new ToolAssemblyDto
        {
            ToolAssemblyId = assembly.ToolAssemblyId,
            AssemblyName = assembly.AssemblyName,
            Description = assembly.Description,
            EspritToolId = assembly.EspritToolId,
            ToolNumber = assembly.ToolNumber,
            IsActive = assembly.IsActive,
            Notes = assembly.Notes,
            Components = assembly.ToolAssemblyComponents.Select(c => new ToolAssemblyComponentDto
            {
                ToolAssemblyComponentId = c.ToolAssemblyComponentId,
                ToolAssemblyId = c.ToolAssemblyId,
                ToolComponentId = c.ToolComponentId,
                QuantityRequired = c.QuantityRequired,
                IsPrimary = c.IsPrimary,
                Notes = c.Notes,
                Component = c.ToolComponent != null ? new ToolComponentDto
                {
                    ToolComponentId = c.ToolComponent.ToolComponentId,
                    ComponentType = c.ToolComponent.ComponentType,
                    ComponentCode = c.ToolComponent.ComponentCode,
                    Description = c.ToolComponent.Description,
                    Manufacturer = c.ToolComponent.Manufacturer,
                    AssetTag = c.ToolComponent.AssetTag
                } : null
            }).ToList()
        };
    }
}
