using Microsoft.EntityFrameworkCore;
using CncTooling.Application.DTOs;
using CncTooling.Domain.Entities;
using CncTooling.Infrastructure.Data;

namespace CncTooling.Application.Services;

public interface IOperatorService
{
    Task<PartDto?> GetPartByNumberAsync(string partNumber);
    Task<List<OperationDto>> GetPartOperationsAsync(string partNumber, string? revisionCode = null);
    Task<OperationDto?> GetOperationAsync(int operationId);
    Task<SetupSheetDto?> GetSetupSheetAsync(int operationId);
    Task<List<ToolAssemblyAvailabilityDto>> GetOperationToolingAsync(int operationId, bool includeLocations = false);
}

public class OperatorService : IOperatorService
{
    private readonly CncToolingDbContext _context;

    public OperatorService(CncToolingDbContext context)
    {
        _context = context;
    }

    public async Task<PartDto?> GetPartByNumberAsync(string partNumber)
    {
        var part = await _context.Parts
            .Include(p => p.DefaultRevision)
            .Include(p => p.Revisions.Where(r => r.IsActive))
                .ThenInclude(r => r.Operations.Where(o => o.IsActive))
            .FirstOrDefaultAsync(p => p.PartNumber == partNumber);

        if (part == null) return null;

        return new PartDto
        {
            PartId = part.PartId,
            PartNumber = part.PartNumber,
            Description = part.Description,
            DefaultRevisionId = part.DefaultRevisionId,
            IsActive = part.IsActive,
            DefaultRevision = part.DefaultRevision != null ? MapRevisionToDto(part.DefaultRevision) : null,
            Revisions = part.Revisions.Select(MapRevisionToDto).ToList()
        };
    }

    public async Task<List<OperationDto>> GetPartOperationsAsync(string partNumber, string? revisionCode = null)
    {
        var query = _context.Operations
            .Include(o => o.PartRevision)
                .ThenInclude(r => r.Part)
            .Where(o => o.PartRevision.Part.PartNumber == partNumber && o.IsActive);

        if (!string.IsNullOrEmpty(revisionCode))
        {
            query = query.Where(o => o.PartRevision.RevisionCode == revisionCode);
        }
        else
        {
            query = query.Where(o => o.PartRevision.IsActive);
        }

        var operations = await query
            .OrderBy(o => o.SequenceNumber)
            .ToListAsync();

        return operations.Select(MapOperationToDto).ToList();
    }

    public async Task<OperationDto?> GetOperationAsync(int operationId)
    {
        var operation = await _context.Operations
            .Include(o => o.PartRevision)
            .FirstOrDefaultAsync(o => o.OperationId == operationId);

        return operation != null ? MapOperationToDto(operation) : null;
    }

    public async Task<SetupSheetDto?> GetSetupSheetAsync(int operationId)
    {
        var setupSheet = await _context.SetupSheets
            .Where(s => s.OperationId == operationId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (setupSheet == null) return null;

        return new SetupSheetDto
        {
            Id = setupSheet.Id,
            SetupSheetId = setupSheet.SetupSheetId,
            OperationId = setupSheet.OperationId,
            FilePath = setupSheet.FilePath,
            Url = setupSheet.Url,
            Format = setupSheet.Format,
            Description = setupSheet.Description,
            IsActive = setupSheet.IsActive
        };
    }

    public async Task<List<ToolAssemblyAvailabilityDto>> GetOperationToolingAsync(int operationId, bool includeLocations = false)
    {
        var operationAssemblies = await _context.OperationToolAssemblies
            .Include(ota => ota.ToolAssembly)
                .ThenInclude(ta => ta.ToolAssemblyComponents)
                    .ThenInclude(tac => tac.ToolComponent)
            .Where(ota => ota.OperationId == operationId)
            .ToListAsync();

        var result = new List<ToolAssemblyAvailabilityDto>();

        foreach (var ota in operationAssemblies)
        {
            var assemblyDto = MapAssemblyToDto(ota.ToolAssembly);
            var componentAvailability = new List<ComponentAvailabilityDto>();

            foreach (var tac in ota.ToolAssembly.ToolAssemblyComponents)
            {
                var inventoryStatuses = await _context.ComponentInventoryStatuses
                    .Include(cis => cis.InventoryLocation)
                    .Include(cis => cis.Machine)
                    .Include(cis => cis.SetupKit)
                    .Where(cis => cis.ToolComponentId == tac.ToolComponentId && 
                                  cis.Status == "Available" || cis.Status == "InMachine" || cis.Status == "InSetupKit")
                    .ToListAsync();

                var quantityAvailable = inventoryStatuses.Sum(s => s.QuantityOnHand);

                var locations = new List<ComponentLocationDto>();
                if (includeLocations)
                {
                    foreach (var status in inventoryStatuses)
                    {
                        locations.Add(new ComponentLocationDto
                        {
                            LocationType = DetermineLocationType(status),
                            LocationDescription = GetLocationDescription(status),
                            Status = status.Status,
                            Quantity = status.QuantityOnHand,
                            MachineInfo = status.Machine?.Name,
                            PocketNumber = status.MachineToolLocation?.PocketNumber
                        });
                    }
                }

                componentAvailability.Add(new ComponentAvailabilityDto
                {
                    Component = MapComponentToDto(tac.ToolComponent),
                    QuantityRequired = tac.QuantityRequired * ota.QuantityRequired,
                    QuantityAvailable = quantityAvailable,
                    IsPrimary = tac.IsPrimary,
                    Locations = locations
                });
            }

            var availabilityStatus = DetermineAvailabilityStatus(componentAvailability);

            result.Add(new ToolAssemblyAvailabilityDto
            {
                Assembly = assemblyDto,
                AvailabilityStatus = availabilityStatus,
                QuantityRequired = ota.QuantityRequired,
                ComponentAvailability = componentAvailability
            });
        }

        return result;
    }

    // Helper methods
    private static PartRevisionDto MapRevisionToDto(PartRevision revision)
    {
        return new PartRevisionDto
        {
            PartRevisionId = revision.PartRevisionId,
            PartId = revision.PartId,
            RevisionCode = revision.RevisionCode,
            IsActive = revision.IsActive,
            Notes = revision.Notes,
            Operations = revision.Operations.Select(MapOperationToDto).ToList()
        };
    }

    private static OperationDto MapOperationToDto(Operation operation)
    {
        return new OperationDto
        {
            OperationId = operation.OperationId,
            PartRevisionId = operation.PartRevisionId,
            OperationName = operation.OperationName,
            EspritProgramName = operation.EspritProgramName,
            EspritProgramId = operation.EspritProgramId,
            SetupSheetPath = operation.SetupSheetPath,
            SetupSheetUrl = operation.SetupSheetUrl,
            IsActive = operation.IsActive,
            SequenceNumber = operation.SequenceNumber,
            Notes = operation.Notes
        };
    }

    private static ToolAssemblyDto MapAssemblyToDto(ToolAssembly assembly)
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
            Components = assembly.ToolAssemblyComponents.Select(tac => new ToolAssemblyComponentDto
            {
                ToolAssemblyComponentId = tac.ToolAssemblyComponentId,
                ToolAssemblyId = tac.ToolAssemblyId,
                ToolComponentId = tac.ToolComponentId,
                QuantityRequired = tac.QuantityRequired,
                IsPrimary = tac.IsPrimary,
                Notes = tac.Notes,
                Component = MapComponentToDto(tac.ToolComponent)
            }).ToList()
        };
    }

    private static ToolComponentDto MapComponentToDto(ToolComponent component)
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

    private static string DetermineLocationType(ComponentInventoryStatus status)
    {
        if (status.MachineId.HasValue) return "Machine";
        if (status.SetupKitId.HasValue) return "SetupKit";
        if (status.InventoryLocationId.HasValue) return "Crib";
        return "Other";
    }

    private static string GetLocationDescription(ComponentInventoryStatus status)
    {
        if (status.Machine != null)
            return $"{status.Machine.Name}{(status.MachineToolLocation != null ? $" - Pocket {status.MachineToolLocation.PocketNumber}" : "")}";
        if (status.SetupKit != null)
            return $"Kit: {status.SetupKit.KitName}";
        if (status.InventoryLocation != null)
            return status.InventoryLocation.LocationCode;
        return "Unknown";
    }

    private static string DetermineAvailabilityStatus(List<ComponentAvailabilityDto> components)
    {
        var primaryComponents = components.Where(c => c.IsPrimary).ToList();
        var allComponents = primaryComponents.Any() ? primaryComponents : components;

        if (allComponents.All(c => c.QuantityAvailable >= c.QuantityRequired))
            return "FullyAvailable";
        if (allComponents.Any(c => c.QuantityAvailable >= c.QuantityRequired))
            return "PartiallyAvailable";
        return "NotAvailable";
    }
}
