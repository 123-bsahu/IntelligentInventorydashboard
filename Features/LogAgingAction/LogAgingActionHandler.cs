using IntelligentInventoryDashboard.Database;
using IntelligentInventoryDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelligentInventoryDashboard.Features.LogAgingAction
{
    /// <summary>
    /// Result returned after logging an aging action.
    /// </summary>
    public record LogAgingActionResult(
        int ActionId,
        int VehicleId,
        string ActionType,
        DateTime CreatedAt);

    /// <summary>
    /// Saves an <see cref="AgingAction"/> record linked to an existing vehicle.
    /// </summary>
    public class LogAgingActionHandler
    {
        private readonly AppDbContext _db;

        public LogAgingActionHandler(AppDbContext db)
        {
            _db = db;
        }

        /// <returns>Null when the vehicle does not exist; otherwise the logged action summary.</returns>
        public async Task<LogAgingActionResult?> Handle(LogAgingActionCommand command, CancellationToken cancellationToken = default)
        {
            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Id == command.VehicleId, cancellationToken);

            if (vehicle is null)
            {
                return null;
            }

            var action = new AgingAction
            {
                VehicleId = vehicle.Id,
                ActionType = command.ActionType,
                Notes = command.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _db.AgingActions.Add(action);
            await _db.SaveChangesAsync(cancellationToken);

            return new LogAgingActionResult(action.Id, vehicle.Id, action.ActionType, action.CreatedAt);
        }
    }
}
