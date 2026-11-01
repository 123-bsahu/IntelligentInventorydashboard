namespace IntelligentInventoryDashboard.Entities
{
    public enum VehicleStatus
    {
        Approved,
        PendingUpdate
    }

    public class Vehicles
    {
        public int Id { get; set; }

        public string ChassisNumber { get; set; } = string.Empty;

        public string Make { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal Price { get; set; }

        public DateOnly ArrivalDate { get; set; }

        public VehicleStatus Status { get; set; } = VehicleStatus.Approved;

       
        public ICollection<VehicleUpdateRequest> UpdateRequests { get; set; } = new List<VehicleUpdateRequest>();

        public ICollection<AgingAction> AgingActions { get; set; } = new List<AgingAction>();

    
        public int GetDaysInStock(DateOnly today) => today.DayNumber - ArrivalDate.DayNumber;
    }
}
