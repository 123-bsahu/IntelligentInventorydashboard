namespace IntelligentInventoryDashboard.Features.GetInventory
{
    /// <summary>
    /// DTO returned to clients for each vehicle in the inventory.
    /// </summary>
    public class GetInventoryResponse
    {
        public int Id { get; set; }

        public string ChassisNumber { get; set; } = string.Empty;

        public string Make { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal Price { get; set; }

        public DateOnly ArrivalDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public int DaysInStock { get; set; }

        public string Recommendation { get; set; } = string.Empty;
    }
}
