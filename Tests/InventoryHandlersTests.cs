using IntelligentInventoryDashboard.Database;
using IntelligentInventoryDashboard.Entities;
using IntelligentInventoryDashboard.Features.ApproveVehicleUpdate;
using IntelligentInventoryDashboard.Features.CreateVehicle;
using IntelligentInventoryDashboard.Features.LogAgingAction;
using IntelligentInventoryDashboard.Features.RequestVehicleUpdate;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IntelligentInventoryDashboard.Tests;

public class InventoryHandlersTests
{
    [Fact]
    public async Task CreateVehicle_CreatesApprovedVehicle()
    {
        await using var db = CreateDatabase();
        var handler = new CreateVehicleHandler(db);

        var result = await handler.Handle(new CreateVehicleCommand
        {
            ChassisNumber = "CHASSIS-001",
            Make = "Ford",
            Model = "Focus",
            Year = 2025,
            Price = 25000m,
            ArrivalDate = new DateOnly(2025, 1, 15)
        });

        Assert.NotEqual(0, result.Id);
        Assert.Equal("CHASSIS-001", result.ChassisNumber);
        Assert.Equal(nameof(VehicleStatus.Approved), result.Status);
        Assert.Equal(DuplicateReason.None, result.Duplicate);
        Assert.Single(db.Vehicles);
    }

    [Fact]
    public async Task CreateVehicle_ReturnsDuplicateWhenChassisAlreadyExists()
    {
        await using var db = CreateDatabase();
        db.Vehicles.Add(CreateVehicle("CHASSIS-001"));
        await db.SaveChangesAsync();

        var handler = new CreateVehicleHandler(db);
        var result = await handler.Handle(new CreateVehicleCommand
        {
            ChassisNumber = "CHASSIS-001",
            Make = "Ford",
            Model = "Focus",
            Year = 2025,
            Price = 25000m,
            ArrivalDate = new DateOnly(2025, 1, 15)
        });

        Assert.Equal(0, result.Id);
        Assert.Equal(DuplicateReason.ChassisNumber, result.Duplicate);
        Assert.Single(db.Vehicles);
    }

    [Fact]
    public async Task RequestUpdate_CreatesPendingRequestAndFlagsVehicle()
    {
        await using var db = CreateDatabase();
        var vehicle = CreateVehicle("CHASSIS-002");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var handler = new RequestUpdateHandler(db);
        var result = await handler.Handle(new RequestUpdateCommand
        {
            VehicleId = vehicle.Id,
            ProposedPrice = 22000m,
            Reason = "Aged stock price review"
        });

        Assert.NotNull(result);
        Assert.NotEqual(0, result!.RequestId);
        Assert.Equal(nameof(UpdateRequestStatus.Pending), result.Status);
        Assert.Equal(nameof(VehicleStatus.PendingUpdate), db.Vehicles.Single().Status.ToString());
        Assert.Equal(UpdateRequestStatus.Pending, db.VehicleUpdateRequests.Single().Status);
    }

    [Fact]
    public async Task ApproveUpdate_AppliesProposedPriceAndApprovesVehicle()
    {
        await using var db = CreateDatabase();
        var vehicle = CreateVehicle("CHASSIS-003");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var requestHandler = new RequestUpdateHandler(db);
        var request = await requestHandler.Handle(new RequestUpdateCommand
        {
            VehicleId = vehicle.Id,
            ProposedPrice = 21000m,
            Reason = "Market adjustment"
        });

        var approvalHandler = new ApproveUpdateHandler(db);
        var result = await approvalHandler.Handle(new ApproveUpdateCommand
        {
            RequestId = request!.RequestId,
            IsApproved = true
        });

        Assert.NotNull(result);
        Assert.Equal(21000m, result!.VehiclePrice);
        Assert.Equal(nameof(VehicleStatus.Approved), result.VehicleStatus);
        Assert.Equal(nameof(UpdateRequestStatus.Approved), result.RequestStatus);
        Assert.NotNull(db.VehicleUpdateRequests.Single().ApprovedAt);
    }

    [Fact]
    public async Task RejectUpdateKeepsOriginalPriceAndApprovesVehicle()
    {
        await using var db = CreateDatabase();
        var vehicle = CreateVehicle("CHASSIS-004");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var requestHandler = new RequestUpdateHandler(db);
        var request = await requestHandler.Handle(new RequestUpdateCommand
        {
            VehicleId = vehicle.Id,
            ProposedPrice = 19000m,
            Reason = "Review requested"
        });

        var approvalHandler = new ApproveUpdateHandler(db);
        var result = await approvalHandler.Handle(new ApproveUpdateCommand
        {
            RequestId = request!.RequestId,
            IsApproved = false
        });

        Assert.NotNull(result);
        Assert.Equal(25000m, result!.VehiclePrice);
        Assert.Equal(nameof(VehicleStatus.Approved), result.VehicleStatus);
        Assert.Equal(nameof(UpdateRequestStatus.Rejected), result.RequestStatus);
        Assert.Null(db.VehicleUpdateRequests.Single().ApprovedAt);
    }

    [Fact]
    public async Task LogAgingAction_CreatesActionForExistingVehicle()
    {
        await using var db = CreateDatabase();
        var vehicle = CreateVehicle("CHASSIS-005");
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        var handler = new LogAgingActionHandler(db);
        var result = await handler.Handle(new LogAgingActionCommand
        {
            VehicleId = vehicle.Id,
            ActionType = "Price Cut",
            Notes = "Reduced price after aging review"
        });

        Assert.NotNull(result);
        Assert.NotEqual(0, result!.ActionId);
        Assert.Equal(vehicle.Id, result.VehicleId);
        Assert.Equal("Price Cut", result.ActionType);
        Assert.Single(db.AgingActions);
    }

    private static AppDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Vehicles CreateVehicle(string chassisNumber)
    {
        return new Vehicles
        {
            ChassisNumber = chassisNumber,
            Make = "Ford",
            Model = "Focus",
            Year = 2025,
            Price = 25000m,
            ArrivalDate = new DateOnly(2025, 1, 15),
            Status = VehicleStatus.Approved
        };
    }
}
