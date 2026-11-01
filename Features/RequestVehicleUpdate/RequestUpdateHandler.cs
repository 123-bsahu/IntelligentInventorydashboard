using IntelligentInventoryDashboard.Database;
using IntelligentInventoryDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelligentInventoryDashboard.Features.RequestVehicleUpdate
{
    /// <summary>
    /// Result returned after a maker submits a price update request.
    /// </summary>
    public record RequestUpdateResult(
        int RequestId,
        int VehicleId,
        decimal ProposedPrice,
        string Status,
        DateTime RequestedAt);

    /// <summary>
    /// Creates a pending <see cref="VehicleUpdateRequest"/> and flags the vehicle as
    /// <see cref="VehicleStatus.PendingUpdate"/> until a checker (senior manager) reviews it.
    /// </summary>
    public class RequestUpdateHandler
    {
        private readonly AppDbContext _db;

        public RequestUpdateHandler(AppDbContext db)
        {
            _db = db;
        }

        /// <returns>Null when the vehicle does not exist; otherwise the created request summary.</returns>
        public async Task<RequestUpdateResult?> Handle(RequestUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Id == command.VehicleId, cancellationToken);

            if (vehicle is null)
            {
                return null;
            }

            var request = new VehicleUpdateRequest
            {
                VehicleId = vehicle.Id,
                ProposedPrice = command.ProposedPrice,
                Reason = command.Reason,
                Status = UpdateRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                ApprovedAt = null
            };

            _db.VehicleUpdateRequests.Add(request);

          
            vehicle.Status = VehicleStatus.PendingUpdate;

            await _db.SaveChangesAsync(cancellationToken);

            return new RequestUpdateResult(
                request.Id,
                vehicle.Id,
                request.ProposedPrice,
                request.Status.ToString(),
                request.RequestedAt);
        }
    }
}
