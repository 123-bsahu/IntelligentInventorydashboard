using IntelligentInventoryDashboard.Database;
using IntelligentInventoryDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelligentInventoryDashboard.Features.ApproveVehicleUpdate
{

    public record ApproveUpdateResult(
        int RequestId,
        int VehicleId,
        decimal VehiclePrice,
        string VehicleStatus,
        string RequestStatus);


    public class ApproveUpdateHandler
    {
        private readonly AppDbContext _db;

        public ApproveUpdateHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ApproveUpdateResult?> Handle(ApproveUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var request = await _db.VehicleUpdateRequests
                .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken);

            if (request is null)
            {
                return null;
            }

            var vehicle = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken);

            if (vehicle is null)
            {
                return null;
            }

            if (command.IsApproved)
            {
                vehicle.Price = request.ProposedPrice;
                vehicle.Status = VehicleStatus.Approved;

                request.Status = UpdateRequestStatus.Approved;
                request.ApprovedAt = DateTime.UtcNow;
            }
            else
            {
                request.Status = UpdateRequestStatus.Rejected;

                
                if (vehicle.Status == VehicleStatus.PendingUpdate)
                {
                    vehicle.Status = VehicleStatus.Approved;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new ApproveUpdateResult(
                request.Id,
                vehicle.Id,
                vehicle.Price,
                vehicle.Status.ToString(),
                request.Status.ToString());
        }
    }
}
