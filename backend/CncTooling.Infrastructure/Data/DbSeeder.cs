using CncTooling.Domain.Entities;
using CncTooling.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CncTooling.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(CncToolingDbContext context)
    {
        // Create database if it doesn't exist
        var created = await context.Database.EnsureCreatedAsync();
        
        if (!created)
            return; // Database already exists with data

        // Seed Machine
        var machine = new Machine
        {
            Name = "MILL 309",
            Model = "CNC Mill",
            IsActive = true
        };
        context.Machines.Add(machine);
        await context.SaveChangesAsync();

        // Seed Part
        var part = new Part
        {
            PartNumber = "2220-0137",
            Description = "WOODWARD-COL Part",
            CreatedAt = DateTime.Parse("2025-08-22"),
            IsActive = true
        };
        context.Parts.Add(part);
        await context.SaveChangesAsync();

        // Seed Part Revision
        var partRevision = new PartRevision
        {
            PartId = part.PartId,
            RevisionNumber = "A",
            RevisionCode = "AJ",
            CreatedDate = DateTime.Parse("2025-08-22"),
            IsActive = true
        };
        context.PartRevisions.Add(partRevision);
        await context.SaveChangesAsync();

        // Seed Operation
        var operation = new Operation
        {
            PartRevisionId = partRevision.Id,
            OperationName = "Mill Operation A",
            SequenceNumber = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Operations.Add(operation);
        await context.SaveChangesAsync();

        // Seed Setup Sheet
        var setupSheet = new SetupSheet
        {
            SetupSheetId = 1,
            OperationId = operation.OperationId,  // Changed from operation.Id
            Instructions = "Replace T47 with T67 when the tool life is reached. DO NOT swap holder. Replace the end mills ONLY.",
            FilePath = "/setup-sheets/2220-0137-A.pdf",
            Format = "PDF",
            Description = "Mill Part Setup - Gray Cast Iron",
            IsActive = true,
            CreatedAt = DateTime.Parse("2025-08-22")
        };
        context.SetupSheets.Add(setupSheet);
        await context.SaveChangesAsync();

        // Seed Tools
        var tools = new[]
        {
            new { Number = 47, Name = "0.5X.01 HOG NOSE", Manufacturer = "GARR 50224 (RGH)", Life = 0, Stickout = 1.5m },
            new { Number = 48, Name = "3/8X90° CENTER DRILL", Manufacturer = "GARR 91030", Life = 0, Stickout = 1.5m },
            new { Number = 48, Name = "#3 JOBBER DRILL", Manufacturer = "0.213X118° DRILL HSS PTD 18603", Life = 225, Stickout = 2.75m },
            new { Number = 50, Name = "1/4-28 UNF TAP", Manufacturer = "OSG 1651501808", Life = 0, Stickout = 1.75m },
            new { Number = 51, Name = "0.375X.03 HOG NOSE", Manufacturer = "HELICAL 43337", Life = 0, Stickout = 1.375m },
            new { Number = 52, Name = "H1/32 SCRW MACH DRILL", Manufacturer = "0.3438X135° CBT PTD 40322", Life = 0, Stickout = 1.938m },
            new { Number = 53, Name = "3510- 3560 BORE", Manufacturer = "0.354", Life = 68, Stickout = 1.875m },
            new { Number = 54, Name = "#5 SCREW MACH DRILL", Manufacturer = "0.2055X135° CBT PTD 41305", Life = 0, Stickout = 1.5m },
            new { Number = 55, Name = "#20 SCREW MACH DRILL", Manufacturer = "0.161X135° CBT PTD 41320", Life = 205, Stickout = 1.313m },
            new { Number = 56, Name = "10-32 UNF TAP", Manufacturer = "OSG 1650501208", Life = 205, Stickout = 1.75m },
            new { Number = 57, Name = "0.25X.01 HOG NOSE", Manufacturer = "GARR 50207", Life = 0, Stickout = 1.0m },
            new { Number = 58, Name = "3.0X0.062 KEYWAY", Manufacturer = "36FL KEO 09270", Life = 80, Stickout = 1.5m },
            new { Number = 59, Name = "0.5X.01 HOG NOSE", Manufacturer = "GARR 50224 (FIN)", Life = 39, Stickout = 1.5m },
            new { Number = 67, Name = "1/2X90° SPOT DRILL", Manufacturer = "GARR 91040", Life = 0, Stickout = 2.0m }
        };

        foreach (var tool in tools)
        {
            var toolEntity = new Tool
            {
                ToolNumber = tool.Number.ToString(),
                Description = tool.Name,
                Type = "Mill Tool",
                Diameter = 0, // Would need to parse from description
                Length = (decimal)tool.Stickout
            };
            context.Tools.Add(toolEntity);
        }
        await context.SaveChangesAsync();

        // Seed Tool Components
        foreach (var tool in tools)
        {
            var toolComponent = new ToolComponent
            {
                ToolId = tool.Number,
                ComponentId = 1,
                ComponentCode = tool.Number.ToString(),
                ComponentType = "Tool",
                Description = tool.Name,
                Manufacturer = tool.Manufacturer,
                AssetTag = $"TOOL-{tool.Number}",
                UnitCost = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.ToolComponents.Add(toolComponent);
        }
        await context.SaveChangesAsync();
    }
}
