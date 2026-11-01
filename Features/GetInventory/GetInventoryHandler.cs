using IntelligentInventoryDashboard.Database;
using IntelligentInventoryDashboard.Entities;
using Microsoft.EntityFrameworkCore;

namespace IntelligentInventoryDashboard.Features.GetInventory
{
    /// <summary>
    /// Loads all vehicles and maps them to <see cref="GetInventoryResponse"/> DTOs, including
    /// the C#-calculated DaysInStock and smart aging recommendations.
    /// </summary>
    public class GetInventoryHandler
    {
        private readonly AppDbContext _db;

        public GetInventoryHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<GetInventoryResponse>> Handle(GetInventoryQuery query, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

           
            var vehicles = await _db.Vehicles
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return vehicles
                .Select(v => MapToResponse(v, today))
                .OrderByDescending(r => r.DaysInStock)
                .ToList();
        }

        private static GetInventoryResponse MapToResponse(Vehicles vehicle, DateOnly today)
        {
            var daysInStock = vehicle.GetDaysInStock(today);

            return new GetInventoryResponse
            {
                Id = vehicle.Id,
                ChassisNumber = vehicle.ChassisNumber,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Price = vehicle.Price,
                ArrivalDate = vehicle.ArrivalDate,
                Status = vehicle.Status.ToString(),
                DaysInStock = daysInStock,
                Recommendation = BuildRecommendation(daysInStock)
            };
        }

        public static string BuildRecommendation(int daysInStock)
        {
            if (daysInStock > 120)
            {
                return "Auction / Dealer Transfer Recommended";
            }

            if (daysInStock > 90)
            {
                return "Price Reduction Recommended";
            }

            return "Healthy Inventory";
        }
    }
}
