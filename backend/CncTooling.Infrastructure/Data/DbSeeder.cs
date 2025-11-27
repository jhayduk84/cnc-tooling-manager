using CncTooling.Domain.Entities;
using CncTooling.Infrastructure.Data;

namespace CncTooling.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(CncToolingDbContext context)
    {
        // Check if data already exists
        if (context.Parts.Any())
            return;

        // Seed Inventory Locations
        var cribLocations = new[]
        {
            new InventoryLocation { LocationCode = "CRIB-A1", Description = "Crib Drawer A1", LocationType = "CribBin", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryLocation { LocationCode = "CRIB-A2", Description = "Crib Drawer A2", LocationType = "CribBin", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryLocation { LocationCode = "CRIB-B1", Description = "Crib Drawer B1", LocationType = "CribBin", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryLocation { LocationCode = "RACK-T1", Description = "Tool Rack T1", LocationType = "CabinetDrawer", IsActive = true, CreatedAt = DateTime.UtcNow },
            new InventoryLocation { LocationCode = "VENDOR", Description = "Out at Vendor", LocationType = "Offsite", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.InventoryLocations.AddRange(cribLocations);
        await context.SaveChangesAsync();

        // Seed Machines
        var machines = new[]
        {
            new Machine { Name = "VF-3 #1", Description = "Haas VF-3 Mill #1", MachineType = "3-axis mill", Manufacturer = "Haas", Model = "VF-3", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Machine { Name = "VF-3 #2", Description = "Haas VF-3 Mill #2", MachineType = "3-axis mill", Manufacturer = "Haas", Model = "VF-3", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Machine { Name = "DMU-50", Description = "DMG DMU-50 5-Axis", MachineType = "5-axis mill", Manufacturer = "DMG", Model = "DMU-50", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.Machines.AddRange(machines);
        await context.SaveChangesAsync();

        // Seed Tool Components
        var components = new[]
        {
            new ToolComponent { ComponentType = "Cutter", ComponentCode = "EM-500-4FL", Description = "0.500\" 4-Flute End Mill", Manufacturer = "Kennametal", AssetTag = null, UnitCost = 45.50m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ToolComponent { ComponentType = "Cutter", ComponentCode = "EM-250-2FL", Description = "0.250\" 2-Flute End Mill", Manufacturer = "OSG", AssetTag = null, UnitCost = 28.75m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ToolComponent { ComponentType = "Holder", ComponentCode = "CAT40-ER32-4", Description = "CAT40 ER32 Collet Chuck 4\" Projection", Manufacturer = "Haimer", AssetTag = "H-1001", UnitCost = 385.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ToolComponent { ComponentType = "Holder", ComponentCode = "CAT40-ER16-3", Description = "CAT40 ER16 Collet Chuck 3\" Projection", Manufacturer = "Haimer", AssetTag = "H-1002", UnitCost = 365.00m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ToolComponent { ComponentType = "Collet", ComponentCode = "ER32-12MM", Description = "ER32 12mm Collet", Manufacturer = "Techniks", AssetTag = null, UnitCost = 18.50m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ToolComponent { ComponentType = "Collet", ComponentCode = "ER16-6MM", Description = "ER16 6mm Collet", Manufacturer = "Techniks", AssetTag = null, UnitCost = 15.50m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ToolComponent { ComponentType = "Insert", ComponentCode = "CCMT-432", Description = "CCMT 21.51 Carbide Insert", Manufacturer = "Sandvik", AssetTag = null, UnitCost = 8.25m, IsActive = true, CreatedAt = DateTime.UtcNow },
            new ToolComponent { ComponentType = "Extension", ComponentCode = "EXT-CAT40-6", Description = "CAT40 6\" Extension", Manufacturer = "Haimer", AssetTag = "E-2001", UnitCost = 225.00m, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.ToolComponents.AddRange(components);
        await context.SaveChangesAsync();

        // Seed Tool Assemblies
        var assembly1 = new ToolAssembly 
        { 
            AssemblyName = "T1 - 1/2\" Rougher", 
            Description = "1/2 inch roughing end mill assembly", 
            ToolNumber = "T1", 
            IsActive = true, 
            CreatedAt = DateTime.UtcNow 
        };
        var assembly2 = new ToolAssembly 
        { 
            AssemblyName = "T2 - 1/4\" Finisher", 
            Description = "1/4 inch finishing end mill assembly", 
            ToolNumber = "T2", 
            IsActive = true, 
            CreatedAt = DateTime.UtcNow 
        };
        context.ToolAssemblies.AddRange(assembly1, assembly2);
        await context.SaveChangesAsync();

        // Link components to assemblies
        var assemblyComponents = new[]
        {
            new ToolAssemblyComponent { ToolAssemblyId = assembly1.ToolAssemblyId, ToolComponentId = components[0].ToolComponentId, QuantityRequired = 1, IsPrimary = true },
            new ToolAssemblyComponent { ToolAssemblyId = assembly1.ToolAssemblyId, ToolComponentId = components[2].ToolComponentId, QuantityRequired = 1, IsPrimary = false },
            new ToolAssemblyComponent { ToolAssemblyId = assembly1.ToolAssemblyId, ToolComponentId = components[4].ToolComponentId, QuantityRequired = 1, IsPrimary = false },
            new ToolAssemblyComponent { ToolAssemblyId = assembly2.ToolAssemblyId, ToolComponentId = components[1].ToolComponentId, QuantityRequired = 1, IsPrimary = true },
            new ToolAssemblyComponent { ToolAssemblyId = assembly2.ToolAssemblyId, ToolComponentId = components[3].ToolComponentId, QuantityRequired = 1, IsPrimary = false },
            new ToolAssemblyComponent { ToolAssemblyId = assembly2.ToolAssemblyId, ToolComponentId = components[5].ToolComponentId, QuantityRequired = 1, IsPrimary = false }
        };
        context.ToolAssemblyComponents.AddRange(assemblyComponents);
        await context.SaveChangesAsync();

        // Seed Parts
        var part1 = new Part 
        { 
            PartNumber = "12345-01", 
            Description = "Mounting Bracket", 
            IsActive = true, 
            CreatedAt = DateTime.UtcNow 
        };
        var part2 = new Part 
        { 
            PartNumber = "67890-02", 
            Description = "Housing Cover", 
            IsActive = true, 
            CreatedAt = DateTime.UtcNow 
        };
        context.Parts.AddRange(part1, part2);
        await context.SaveChangesAsync();

        // Seed Part Revisions
        var revision1 = new PartRevision 
        { 
            PartId = part1.PartId, 
            RevisionCode = "A", 
            IsActive = true, 
            Notes = "Initial release", 
            CreatedAt = DateTime.UtcNow 
        };
        var revision2 = new PartRevision 
        { 
            PartId = part2.PartId, 
            RevisionCode = "B", 
            IsActive = true, 
            Notes = "Updated dimensions", 
            CreatedAt = DateTime.UtcNow 
        };
        context.PartRevisions.AddRange(revision1, revision2);
        await context.SaveChangesAsync();

        // Update default revisions
        part1.DefaultRevisionId = revision1.PartRevisionId;
        part2.DefaultRevisionId = revision2.PartRevisionId;
        await context.SaveChangesAsync();

        // Seed Operations
        var operation1 = new Operation 
        { 
            PartRevisionId = revision1.PartRevisionId, 
            OperationName = "OP10", 
            SequenceNumber = 10,
            EspritProgramName = "12345-01-OP10.ESP",
            SetupSheetUrl = "/setup-sheets/12345-01-OP10.pdf",
            IsActive = true, 
            CreatedAt = DateTime.UtcNow 
        };
        var operation2 = new Operation 
        { 
            PartRevisionId = revision1.PartRevisionId, 
            OperationName = "OP20", 
            SequenceNumber = 20,
            EspritProgramName = "12345-01-OP20.ESP",
            SetupSheetUrl = "/setup-sheets/12345-01-OP20.pdf",
            IsActive = true, 
            CreatedAt = DateTime.UtcNow 
        };
        var operation3 = new Operation 
        { 
            PartRevisionId = revision2.PartRevisionId, 
            OperationName = "OP10", 
            SequenceNumber = 10,
            EspritProgramName = "67890-02-OP10.ESP",
            SetupSheetUrl = "/setup-sheets/67890-02-OP10.pdf",
            IsActive = true, 
            CreatedAt = DateTime.UtcNow 
        };
        context.Operations.AddRange(operation1, operation2, operation3);
        await context.SaveChangesAsync();

        // Seed Setup Sheets
        var setupSheets = new[]
        {
            new SetupSheet { OperationId = operation1.OperationId, FilePath = "/setup-sheets/12345-01-OP10.pdf", Format = "PDF", Description = "OP10 Setup Sheet", IsActive = true, CreatedAt = DateTime.UtcNow },
            new SetupSheet { OperationId = operation2.OperationId, FilePath = "/setup-sheets/12345-01-OP20.pdf", Format = "PDF", Description = "OP20 Setup Sheet", IsActive = true, CreatedAt = DateTime.UtcNow },
            new SetupSheet { OperationId = operation3.OperationId, FilePath = "/setup-sheets/67890-02-OP10.pdf", Format = "PDF", Description = "OP10 Setup Sheet", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.SetupSheets.AddRange(setupSheets);
        await context.SaveChangesAsync();

        // Link assemblies to operations
        var operationAssemblies = new[]
        {
            new OperationToolAssembly { OperationId = operation1.OperationId, ToolAssemblyId = assembly1.ToolAssemblyId, QuantityRequired = 1, IsRequired = true },
            new OperationToolAssembly { OperationId = operation1.OperationId, ToolAssemblyId = assembly2.ToolAssemblyId, QuantityRequired = 1, IsRequired = true },
            new OperationToolAssembly { OperationId = operation2.OperationId, ToolAssemblyId = assembly2.ToolAssemblyId, QuantityRequired = 1, IsRequired = true },
            new OperationToolAssembly { OperationId = operation3.OperationId, ToolAssemblyId = assembly1.ToolAssemblyId, QuantityRequired = 1, IsRequired = true }
        };
        context.OperationToolAssemblies.AddRange(operationAssemblies);
        await context.SaveChangesAsync();

        // Seed component inventory status
        var inventoryStatuses = new[]
        {
            new ComponentInventoryStatus { ToolComponentId = components[0].ToolComponentId, InventoryLocationId = cribLocations[0].InventoryLocationId, Status = "Available", QuantityOnHand = 5, LastMovementAt = DateTime.UtcNow },
            new ComponentInventoryStatus { ToolComponentId = components[1].ToolComponentId, InventoryLocationId = cribLocations[0].InventoryLocationId, Status = "Available", QuantityOnHand = 8, LastMovementAt = DateTime.UtcNow },
            new ComponentInventoryStatus { ToolComponentId = components[2].ToolComponentId, InventoryLocationId = cribLocations[1].InventoryLocationId, Status = "Available", QuantityOnHand = 2, LastMovementAt = DateTime.UtcNow },
            new ComponentInventoryStatus { ToolComponentId = components[3].ToolComponentId, InventoryLocationId = cribLocations[1].InventoryLocationId, Status = "Available", QuantityOnHand = 3, LastMovementAt = DateTime.UtcNow },
            new ComponentInventoryStatus { ToolComponentId = components[4].ToolComponentId, InventoryLocationId = cribLocations[2].InventoryLocationId, Status = "Available", QuantityOnHand = 12, LastMovementAt = DateTime.UtcNow },
            new ComponentInventoryStatus { ToolComponentId = components[5].ToolComponentId, InventoryLocationId = cribLocations[2].InventoryLocationId, Status = "Available", QuantityOnHand = 15, LastMovementAt = DateTime.UtcNow },
            new ComponentInventoryStatus { ToolComponentId = components[0].ToolComponentId, MachineId = machines[0].MachineId, Status = "InMachine", QuantityOnHand = 1, LastMovementAt = DateTime.UtcNow }
        };
        context.ComponentInventoryStatuses.AddRange(inventoryStatuses);
        await context.SaveChangesAsync();

        // Seed machine tool locations
        var machineLocations = new[]
        {
            new MachineToolLocation { MachineId = machines[0].MachineId, PocketNumber = 1, ToolAssemblyId = assembly1.ToolAssemblyId, LastUpdated = DateTime.UtcNow },
            new MachineToolLocation { MachineId = machines[0].MachineId, PocketNumber = 2, ToolAssemblyId = assembly2.ToolAssemblyId, LastUpdated = DateTime.UtcNow },
            new MachineToolLocation { MachineId = machines[1].MachineId, PocketNumber = 1, LastUpdated = DateTime.UtcNow }
        };
        context.MachineToolLocations.AddRange(machineLocations);
        await context.SaveChangesAsync();

        // Seed Setup Kit
        var setupKit = new SetupKit 
        { 
            KitName = "Kit-12345-01", 
            PartRevisionId = revision1.PartRevisionId, 
            OperationId = operation1.OperationId,
            Status = "Planned",
            CreatedAt = DateTime.UtcNow 
        };
        context.SetupKits.Add(setupKit);
        await context.SaveChangesAsync();

        var kitItems = new[]
        {
            new SetupKitItem { SetupKitId = setupKit.SetupKitId, ToolAssemblyId = assembly1.ToolAssemblyId, QuantityPlanned = 1, QuantityPulled = 0, Status = "Pending" },
            new SetupKitItem { SetupKitId = setupKit.SetupKitId, ToolAssemblyId = assembly2.ToolAssemblyId, QuantityPlanned = 1, QuantityPulled = 0, Status = "Pending" }
        };
        context.SetupKitItems.AddRange(kitItems);
        await context.SaveChangesAsync();
    }
}
