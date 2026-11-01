using IntelligentInventoryDashboard.Database;
using IntelligentInventoryDashboard.Entities;
using IntelligentInventoryDashboard.Features.GetInventory;
using IntelligentInventoryDashboard.Services;
using Microsoft.EntityFrameworkCore;

namespace IntelligentInventoryDashboard.Features.SearchVehicles
{
    /// <summary>
    /// Builds a dynamic EF Core query from NLP-extracted <see cref="VehicleQueryFilters"/>:
    /// Make, Model, MaxPrice, and MinDays filters are applied when present, with the
    /// Keyword used as a fallback match against chassis number, make, and model.
    /// </summary>
    public class SearchVehiclesHandler
    {
        private readonly AppDbContext _db;

        public SearchVehiclesHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<GetInventoryResponse>> Handle(VehicleQueryFilters filters, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var vehicles = _db.Vehicles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filters.Make))
            {
                var make = filters.Make;
                vehicles = vehicles.Where(v => v.Make.ToLower().Contains(make.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filters.Model))
            {
                var model = filters.Model;
                vehicles = vehicles.Where(v => v.Model.ToLower().Contains(model.ToLower()));
            }

            if (filters.MaxPrice is decimal maxPrice)
            {
                vehicles = vehicles.Where(v => v.Price <= maxPrice);
            }

            if (filters.MinDays is int minDays && minDays > 0)
            {
                // Translated as a SQL date comparison: arrival must be at least minDays before today.
                // (DayNumber arithmetic itself cannot be translated to SQL.)
                var cutoff = today.AddDays(-minDays);
                vehicles = vehicles.Where(v => v.ArrivalDate <= cutoff);
            }

            var keyword = filters.Keyword?.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                // Parameterized LIKE '%keyword%' with EF Core wildcard escaping; SQL Server's
                // default collation makes it case-insensitive.
                vehicles = vehicles.Where(v =>
                    v.ChassisNumber.Contains(keyword) ||
                    v.Make.Contains(keyword) ||
                    v.Model.Contains(keyword));
            }

            var matched = await vehicles.ToListAsync(cancellationToken);

            return matched
                .Select(v => MapToResponse(v, today))
                .OrderByDescending(r => r.DaysInStock)
                .ToList();
        }

        private static GetInventoryResponse MapToResponse(Vehicles vehicle, DateOnly today) => new()
        {
            Id = vehicle.Id,
            ChassisNumber = vehicle.ChassisNumber,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            ArrivalDate = vehicle.ArrivalDate,
            Status = vehicle.Status.ToString(),
            DaysInStock = vehicle.GetDaysInStock(today),
            Recommendation = GetInventoryHandler.BuildRecommendation(vehicle.GetDaysInStock(today))
        };
    }
}
