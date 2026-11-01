using IntelligentInventoryDashboard.Database;
using IntelligentInventoryDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelligentInventoryDashboard.Features.CreateVehicle
{
  
    public record CreateVehicleResult(
        int Id,
        string ChassisNumber,
        string Status,
        DuplicateReason Duplicate);

    public enum DuplicateReason
    {
        None,
        ChassisNumber
    }

    /// <summary>
    /// Creates a new <see cref="Vehicles"/> record with the default status <see cref="VehicleStatus.Approved"/>.
    /// </summary>
    public class CreateVehicleHandler
    {
        private readonly AppDbContext _db;

        public CreateVehicleHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CreateVehicleResult> Handle(CreateVehicleCommand command, CancellationToken cancellationToken = default)
        {
            // The schema enforces a unique index on ChassisNumber; check up front for a friendlier 409.
            var chassisTaken = await _db.Vehicles
                .AnyAsync(v => v.ChassisNumber == command.ChassisNumber, cancellationToken);

            if (chassisTaken)
            {
                return new CreateVehicleResult(0, command.ChassisNumber, VehicleStatus.Approved.ToString(), DuplicateReason.ChassisNumber);
            }

            var vehicle = new Vehicles
            {
                ChassisNumber = command.ChassisNumber,
                Make = command.Make,
                Model = command.Model,
                Year = command.Year,
                Price = command.Price,
                ArrivalDate = command.ArrivalDate,
                Status = VehicleStatus.Approved // default status for newly arrived stock
            };

            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync(cancellationToken);

            return new CreateVehicleResult(vehicle.Id, vehicle.ChassisNumber, vehicle.Status.ToString(), DuplicateReason.None);
        }
    }
}
